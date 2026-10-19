using System.Collections.Generic;

namespace Stump.Server.WorldServer.AI.Fights.Brain
{
    /// <summary>
    /// MYSTIC 19/10 : monstres « fuyards à portée » (validé par l'utilisateur) : les 139 monstres (hors boss) dont AUCUN sort de
    /// dégâts ne frappe au contact (portée minimale ≥ 2) et qui ont une attaque à 4 cases ou plus (Monsters.d2o / SpellLevels.d2o 2.51,
    /// niveau 1 des sorts). Après leurs sorts, ils s'éloignent de l'ennemi le plus proche en restant à portée (Brain.FuirAPortee).
    /// Liste générée par _OUTILS/serveur/ia/classer_ia_monstres.py.
    /// </summary>
    public static class MonstresFuyards
    {
        public static readonly HashSet<int> Ids = new HashSet<int>
        {
            181, 385, 389, 426, 675, 681, 748, 751, 752, 753, 754, 829, 1039, 1049, 1075, 2287,
            2360, 2391, 2509, 2513, 2516, 2517, 2518, 2525, 2545, 2855, 2936, 2970, 3149, 3154, 3156, 3159,
            3164, 3182, 3187, 3188, 3192, 3197, 3375, 3376, 3379, 3387, 3408, 3427, 3522, 3538, 3570, 3655,
            3656, 3662, 3669, 3675, 3677, 3680, 3690, 3737, 3738, 3750, 3768, 3782, 3783, 3785, 3789, 3825,
            3844, 3855, 3856, 3858, 3860, 3861, 3863, 3866, 3880, 3881, 3904, 3906, 3971, 3981, 4137, 4165,
            4190, 4294, 4328, 4338, 4440, 4469, 4480, 4490, 4504, 4585, 4587, 4616, 4636, 4687, 4694, 4699,
            4818, 4871, 4872, 4873, 4874, 4969, 4987, 5061, 5081, 5150, 5151, 5183, 5212, 5283, 5315, 5318,
            5325, 5330, 5349, 5350, 5352, 5354, 5357, 5361, 5365, 5368, 5419, 5438, 5458, 5475, 5478, 5479,
            5680, 5681, 5702, 5716, 5717, 5718, 5721, 5723, 5760, 5785, 5787,
        };
    }
}
