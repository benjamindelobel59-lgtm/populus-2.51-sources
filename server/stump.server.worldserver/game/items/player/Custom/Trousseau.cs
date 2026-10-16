using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.WorldServer.Database.Items;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Accounts;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Dialogs.Interactives;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Maps;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Items.Player.Custom
{
    [ItemId(ItemIdEnum.TROUSSEAU_DE_CLEFS_10207)]
    public class Trousseau : BasePlayerItem
    {
        public Trousseau(Character owner, PlayerItemRecord record)
            : base(owner, record)
        {
        }

        // MYSTIC : plus de panneau de donjons. Le trousseau contient toutes les clefs : une entree gratuite par donjon a clef,
        // utilisee automatiquement chez le gardien (DungeonEnterReply), reinitialisee tous les 7 jours (TrousseauManager).
        // Utiliser l'objet affiche seulement son etat.
        public override uint UseItem(int amount = 0, Cell targetCell = null, Character target = null)
        {
            var etat = TrousseauManager.Instance.GetEtat(Owner);
            int utilises;
            DateTime resetAt;
            lock (etat)
            {
                utilises = etat.Utilises.Count;
                resetAt = etat.ResetAt;
            }

            Owner.SendServerMessage(utilises == 0
                ? "Trousseau de clefs : toutes les clefs sont disponibles (une entrée gratuite par donjon)."
                : string.Format("Trousseau de clefs : {0} donjon(s) déjà utilisé(s), toutes les clefs reviennent le {1:dd/MM à HH:mm}.", utilises, resetAt));

            return 0;
        }
    }
}
