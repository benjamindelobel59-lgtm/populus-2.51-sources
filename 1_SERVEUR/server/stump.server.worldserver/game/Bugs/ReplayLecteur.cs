using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.Core.Attributes;
using Stump.Core.Timers;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Handlers.Context;
using Stump.Server.WorldServer.Handlers.Context.RolePlay;

namespace Stump.Server.WorldServer.Game.Bugs
{
    /// <summary>
    /// PADGREF 23/10 : replay EXACT d'un combat dans le client Dofus. Le joueur passe en spectateur d'un combat « fantôme » :
    /// le serveur lui renvoie, avec leur rythme d'origine, les messages enregistrés par ReplayCombat (vrais personnages,
    /// équipements, sorts, animations). Bouton « Quitter » du combat ou .replay stop pour arrêter ; à la fin, retour sur la carte.
    ///   1. Démarrer : le personnage quitte sa carte (comme un spectateur), contexte combat, chargement de la carte du combat ;
    ///   2. ContextePret (GameContextReady) : infos de la carte (sans acteurs), début de combat en spectateur, puis les messages ;
    ///   3. Terminer : contexte roleplay, le personnage revient sur sa carte.
    /// </summary>
    public static class ReplayLecteur
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>Attente maximale entre deux messages pendant la relecture (ms) : les temps de réflexion sont raccourcis.</summary>
        [Variable(true)]
        public static int AttenteMaxMs = 1500;

        /// <summary>Vitesse de relecture (1 = vitesse réelle, 2 = deux fois plus vite).</summary>
        [Variable(true)]
        public static double Vitesse = 1.0;

        class Lecture
        {
            public Character Perso;
            public int Carte;
            public int TypeCombat;
            public double Attaquant, Defenseur;
            public List<KeyValuePair<int, Message>> Messages;
            public int Index;
            public bool Pret;
            public TimedTimerEntry Minuteur;
        }

        static readonly ConcurrentDictionary<int, Lecture> m_lectures = new ConcurrentDictionary<int, Lecture>();

        public static bool EnCours(Character perso) => perso != null && m_lectures.ContainsKey(perso.Id);

        /// <summary>Lance la relecture d'un replay (JSON compressé de ReplayCombat) ; erreur = raison du refus.</summary>
        public static bool Demarrer(Character perso, byte[] donnees, out string erreur)
        {
            erreur = null;
            if (perso.IsFighting() || perso.IsSpectator() || !perso.IsInWorld || perso.Map == null)
            {
                erreur = "impossible pendant un combat.";
                return false;
            }
            if (perso.IsBusy() || EnCours(perso))
            {
                erreur = "termine d'abord ce que tu fais (dialogue, échange ou replay).";
                return false;
            }

            Newtonsoft.Json.Linq.JObject infos;
            List<KeyValuePair<int, Message>> messages;
            try
            {
                infos = ReplayCombat.Ouvrir(donnees, out messages);
            }
            catch (Exception ex)
            {
                logger.Error("[Replay] lecture impossible : {0}", ex.Message);
                erreur = "replay illisible.";
                return false;
            }

            var carte = (int?)infos["carte"] ?? 0;
            if (messages.Count == 0 || World.Instance.GetMap(carte) == null)
            {
                erreur = "replay vide ou carte inconnue.";
                return false;
            }

            var l = new Lecture
            {
                Perso = perso,
                Carte = carte,
                TypeCombat = (int?)infos["typeId"] ?? 4,
                Attaquant = (double?)infos["attaquant"] ?? 0,
                Defenseur = (double?)infos["defenseur"] ?? 0,
                Messages = messages
            };
            m_lectures[perso.Id] = l;
            perso.LoggedOut += OnDeconnexion;

            // comme un spectateur : on quitte la carte (les autres joueurs ne le voient plus) sans changer de carte
            perso.NextMap = perso.Map;
            perso.Map.Leave(perso);
            if (perso.IsInMovement)
                perso.StopMove();

            ContextHandler.SendGameContextDestroyMessage(perso.Client);
            ContextHandler.SendGameContextCreateMessage(perso.Client, 2);
            ContextRoleplayHandler.SendCurrentMapMessage(perso.Client, carte);

            // sécurité : si le client ne confirme jamais le chargement de la carte, on commence quand même
            l.Minuteur = perso.Map.Area.CallDelayed(15000, () => ContextePret(perso));
            logger.Info("[Replay] {0} : relecture en jeu ({1} messages, carte {2})", perso.Name, messages.Count, carte);
            return true;
        }

        /// <summary>GameContextReady du client : la carte du combat est chargée -> début de la relecture.</summary>
        public static void ContextePret(Character perso)
        {
            if (!m_lectures.TryGetValue(perso.Id, out var l) || l.Pret)
                return;
            l.Pret = true;
            l.Minuteur?.Dispose();

            try
            {
                // ferme l'écran de chargement : informations de la carte du combat, sans aucun acteur ni combat posé dessus
                var carte = World.Instance.GetMap(l.Carte);
                var mcid = carte.GetMapComplementaryInformationsDataMessage(perso);
                mcid.Actors = new GameRolePlayActorInformations[0];
                mcid.Fights = new FightCommonInformations[0];
                perso.Client.Send(mcid);

                perso.Client.Send(new GameFightStartingMessage((sbyte)l.TypeCombat, 0, l.Attaquant, l.Defenseur));
                perso.Client.Send(new GameFightSpectatorJoinMessage(false, false, false, false, 0, (sbyte)l.TypeCombat, new NamedPartyTeam[0]));
                perso.SendServerMessage("<b>Replay</b> : lecture du combat (bouton « Quitter » pour arrêter).", System.Drawing.Color.Orange);
            }
            catch (Exception ex)
            {
                logger.Error("[Replay] {0} : début impossible : {1}", perso.Name, ex);
                Terminer(perso, "erreur au démarrage du replay");
                return;
            }

            Suivant(l, 0);
        }

        static void Suivant(Lecture l, int attente)
        {
            l.Minuteur = l.Perso.Map.Area.CallDelayed(Math.Max(1, attente), () => Envoyer(l));
        }

        static void Envoyer(Lecture l)
        {
            if (!m_lectures.TryGetValue(l.Perso.Id, out var actuelle) || actuelle != l)
                return;
            if (!l.Perso.IsLoggedIn || l.Perso.Client == null || !l.Perso.Client.Connected)
            {
                Arreter(l.Perso, false);
                return;
            }

            // tous les messages du même instant partent ensemble (une séquence d'actions entière)
            var debut = l.Messages[l.Index].Key;
            try
            {
                while (l.Index < l.Messages.Count && l.Messages[l.Index].Key - debut < 15)
                    l.Perso.Client.Send(l.Messages[l.Index++].Value);
            }
            catch (Exception ex)
            {
                logger.Error("[Replay] {0} : envoi impossible : {1}", l.Perso.Name, ex.Message);
                Terminer(l.Perso, "erreur pendant le replay");
                return;
            }

            if (l.Index >= l.Messages.Count)
            {
                // laisse le temps de lire l'écran de fin de combat
                l.Minuteur = l.Perso.Map.Area.CallDelayed(4000, () => Terminer(l.Perso, "fin du replay"));
                return;
            }

            var ecart = l.Messages[l.Index].Key - l.Messages[l.Index - 1].Key;
            Suivant(l, (int)(Math.Min(ecart, AttenteMaxMs) / Math.Max(0.1, Vitesse)));
        }

        /// <summary>Bouton « Quitter » du combat ou .replay stop.</summary>
        public static bool Arreter(Character perso, bool revenir = true)
        {
            if (!m_lectures.ContainsKey(perso.Id))
                return false;
            if (revenir)
                Terminer(perso, "replay arrêté");
            else
                Nettoyer(perso);
            return true;
        }

        static void Nettoyer(Character perso)
        {
            if (m_lectures.TryRemove(perso.Id, out var l))
                l.Minuteur?.Dispose();
            perso.LoggedOut -= OnDeconnexion;
        }

        static void OnDeconnexion(Character perso) => Nettoyer(perso);

        static void Terminer(Character perso, string raison)
        {
            Nettoyer(perso);
            if (!perso.IsLoggedIn)
                return;

            ContextHandler.SendGameContextDestroyMessage(perso.Client);
            ContextHandler.SendGameContextCreateMessage(perso.Client, 1);

            var carte = perso.NextMap ?? perso.Map;
            if (carte == null)
                return;
            if (!carte.Area.IsRunning)
                carte.Area.Start();
            carte.Area.ExecuteInContext(() =>
            {
                if (!perso.IsLoggedIn)
                    return;
                perso.LastMap = perso.Map;
                perso.Map = carte;
                perso.NextMap = null;
                perso.Map.Enter(perso);
                perso.SendServerMessage("Replay : " + raison + ".");
            });
            logger.Info("[Replay] {0} : {1}", perso.Name, raison);
        }
    }
}
