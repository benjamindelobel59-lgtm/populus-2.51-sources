using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Spells.Casts;

namespace Stump.Server.WorldServer.Game.Bugs
{
    /// <summary>
    /// MYSTIC 17/10 : journal texte de chaque combat (combattants, tours, sorts lancés, dommages / soins, états, morts), gardé en
    /// mémoire pour le bouton « Signaler un bug » : un joueur envoie le combat en cours, ou son dernier combat terminé.
    /// </summary>
    public static class JournalCombat
    {
        const int MaxLignes = 4000;

        static readonly ConcurrentDictionary<IFight, Journal> m_enCours = new ConcurrentDictionary<IFight, Journal>();
        static readonly ConcurrentDictionary<int, string> m_dernierParPerso = new ConcurrentDictionary<int, string>();

        class Journal
        {
            public readonly List<string> Lignes = new List<string>();
            public readonly DateTime Debut = DateTime.Now;
            public int Tour;
            public bool Coupe;

            public void Ajouter(string ligne)
            {
                lock (Lignes)
                {
                    if (Lignes.Count >= MaxLignes)
                    {
                        if (!Coupe)
                            Lignes.Add("... (journal coupé à " + MaxLignes + " lignes)");
                        Coupe = true;
                        return;
                    }
                    Lignes.Add(DateTime.Now.ToString("HH:mm:ss") + " " + ligne);
                }
            }

            public string Texte()
            {
                lock (Lignes)
                    return string.Join("\n", Lignes);
            }
        }

        /// <summary>Appelé à la création de chaque combat (Fight).</summary>
        public static void Suivre(IFight fight)
        {
            var j = new Journal();
            m_enCours[fight] = j;
            j.Ajouter($"Combat {fight.Id} ({fight.FightType}) sur la carte {fight.Map.Id}");

            foreach (var team in new[] { fight.ChallengersTeam, fight.DefendersTeam })
                team.FighterAdded += (t, f) => SuivreCombattant(j, f, true);

            fight.FightStarted += f =>
            {
                j.Ajouter("--- Début du combat ---");
                foreach (var c in f.GetAllFighters())
                    j.Ajouter("  " + Decrire(c) + $" équipe {c.Team.Id} case {c.Cell?.Id} PV {c.LifePoints}/{c.MaxLifePoints} PA {c.AP} PM {c.MP}");
            };
            fight.TurnStarted += (f, c) =>
            {
                j.Tour = f.TimeLine != null ? f.TimeLine.RoundNumber : j.Tour;
                j.Ajouter($"[Tour {j.Tour}] début du tour de {Decrire(c)} (case {c.Cell?.Id}, PV {c.LifePoints}/{c.MaxLifePoints}, PA {c.AP}, PM {c.MP})");
            };
            fight.FightEnded += f =>
            {
                j.Ajouter("--- Fin du combat ---");
                var texte = Entete(f, j) + j.Texte();
                foreach (var c in f.GetAllFighters().OfType<CharacterFighter>())
                    m_dernierParPerso[c.Character.Id] = texte;
                m_enCours.TryRemove(f, out _);
            };
        }

        static void SuivreCombattant(Journal j, FightActor c, bool annoncer)
        {
            if (annoncer && c.Fight != null && c.Fight.State == FightState.Fighting)
                j.Ajouter("  arrivée de " + Decrire(c) + $" case {c.Cell?.Id} PV {c.LifePoints}/{c.MaxLifePoints}");

            c.SpellCasted += (lanceur, cast) => j.Ajouter($"  {Decrire(lanceur)} lance {NomSort(cast)} sur la case {cast.TargetedCell?.Id}" + (cast.Critical ? " (coup critique)" : ""));
            c.SpellCastFailed += (lanceur, infos) => j.Ajouter($"  {Decrire(lanceur)} : échec du lancer de {infos.Spell?.Id} sur la case {infos.TargetedCell?.Id}");
            c.LifePointsChanged += (cible, delta, bouclier, erosion, source, ecole, dommage) =>
                j.Ajouter($"  {Decrire(cible)} {(delta < 0 ? "perd " + (-delta) : "gagne " + delta)} PV ({ecole}{(source != null ? ", par " + Decrire(source) : "")}{(bouclier > 0 ? ", bouclier " + bouclier : "")}) -> {cible.LifePoints}/{cible.MaxLifePoints}");
            c.BuffAdded += (cible, buff) =>
            {
                if (buff is StateBuff s && s.State != null)
                    j.Ajouter($"  {Decrire(cible)} prend l'état {s.State.Id} {s.State.Name} (sort {buff.Spell?.Id}, durée {buff.Duration})");
            };
            c.BuffRemoved += (cible, buff) =>
            {
                if (buff is StateBuff s && s.State != null)
                    j.Ajouter($"  {Decrire(cible)} perd l'état {s.State.Id} {s.State.Name}");
            };
            c.Dead += (mort, tueur) => j.Ajouter($"  MORT de {Decrire(mort)}" + (tueur != null && tueur != mort ? " (par " + Decrire(tueur) + ")" : ""));
        }

        static string Entete(IFight f, Journal j) =>
            $"Journal du combat {f.Id} - carte {f.Map.Id} - {f.FightType} - début {j.Debut:dd/MM/yyyy HH:mm:ss}\n\n";

        static string NomSort(SpellCastHandler cast)
        {
            var s = cast.Spell;
            if (s == null)
                return "?";
            return $"{s.Template?.Name} ({s.Id} niv. {s.CurrentLevel})";
        }

        static string Decrire(FightActor c)
        {
            if (c == null)
                return "?";
            string nom;
            try
            {
                nom = c.GetMapRunningFighterName();
            }
            catch
            {
                nom = c.GetType().Name;
            }
            switch (c)
            {
                case CharacterFighter p:
                    return $"{nom} (perso {p.Character.Id})";
                case MonsterFighter m:
                    return $"{nom} (monstre {m.Monster.Template.Id}, id {c.Id})";
                case SummonedMonster s:
                    return $"{nom} (invocation {s.Monster.MonsterId}, id {c.Id})";
                default:
                    return $"{nom} (id {c.Id})";
            }
        }

        /// <summary>Journal du combat en cours du personnage, sinon de son dernier combat terminé (null s'il n'y en a pas).</summary>
        public static string Pour(Character perso)
        {
            if (perso.IsFighting() && perso.Fight != null && m_enCours.TryGetValue(perso.Fight, out var j))
                return Entete(perso.Fight, j) + j.Texte() + "\n(combat encore en cours au moment du signalement)";

            return m_dernierParPerso.TryGetValue(perso.Id, out var texte) ? texte : null;
        }
    }
}
