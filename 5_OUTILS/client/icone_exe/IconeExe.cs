// MYSTIC 18/10/2026 : remplace l'icône principale d'un .exe Windows par un fichier .ico (toutes ses tailles).
// Utilise l'API Windows (BeginUpdateResource / UpdateResource / EndUpdateResource), comme Resource Hacker.
// Usage : IconeExe.exe <fichier.exe> <icone.ico>
// Le premier groupe d'icônes de l'exe (celui qu'affiche Windows) est remplacé, avec la même langue ;
// les images sont écrites sous de nouveaux numéros pour ne rien écraser d'autre.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

static class IconeExe
{
    const int RT_ICON = 3, RT_GROUP_ICON = 14;

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)] static extern IntPtr BeginUpdateResource(string fichier, bool supprimerTout);
    [DllImport("kernel32.dll", SetLastError = true)] static extern bool UpdateResource(IntPtr h, IntPtr type, IntPtr nom, ushort langue, byte[] donnees, uint taille);
    [DllImport("kernel32.dll", SetLastError = true)] static extern bool EndUpdateResource(IntPtr h, bool annuler);
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)] static extern IntPtr LoadLibraryEx(string fichier, IntPtr r, uint options);
    [DllImport("kernel32.dll")] static extern bool FreeLibrary(IntPtr h);
    delegate bool EnumNom(IntPtr module, IntPtr type, IntPtr nom, IntPtr param);
    delegate bool EnumLangue(IntPtr module, IntPtr type, IntPtr nom, ushort langue, IntPtr param);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] static extern bool EnumResourceNames(IntPtr module, IntPtr type, EnumNom f, IntPtr param);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] static extern bool EnumResourceLanguages(IntPtr module, IntPtr type, IntPtr nom, EnumLangue f, IntPtr param);
    [DllImport("kernel32.dll")] static extern IntPtr FindResourceEx(IntPtr module, IntPtr type, IntPtr nom, ushort langue);
    [DllImport("kernel32.dll")] static extern IntPtr LoadResource(IntPtr module, IntPtr res);
    [DllImport("kernel32.dll")] static extern IntPtr LockResource(IntPtr res);
    [DllImport("kernel32.dll")] static extern uint SizeofResource(IntPtr module, IntPtr res);

    static int Main(string[] args)
    {
        if (args.Length != 2) { Console.Error.WriteLine("Usage : IconeExe.exe <fichier.exe> <icone.ico>"); return 1; }
        string exe = Path.GetFullPath(args[0]), ico = Path.GetFullPath(args[1]);

        // ---- lecture du .ico : en-tête (6 octets) + entrées de 16 octets + images
        var b = File.ReadAllBytes(ico);
        int n = BitConverter.ToUInt16(b, 4);
        var images = new List<byte[]>();
        var entrees = new List<byte[]>();
        for (int i = 0; i < n; i++)
        {
            int o = 6 + i * 16;
            int taille = BitConverter.ToInt32(b, o + 8), debut = BitConverter.ToInt32(b, o + 12);
            var img = new byte[taille];
            Buffer.BlockCopy(b, debut, img, 0, taille);
            images.Add(img);
            var e = new byte[12];
            Buffer.BlockCopy(b, o, e, 0, 12); // largeur, hauteur, couleurs, réservé, plans, bits, taille
            entrees.Add(e);
        }

        // ---- groupe d'icônes existant (nom + langue) et plus grand numéro d'image déjà utilisé
        IntPtr nomGroupe = (IntPtr)1; ushort langue = 1033; int maxIcone = 0; bool trouve = false;
        var m = LoadLibraryEx(exe, IntPtr.Zero, 0x2 /* LOAD_LIBRARY_AS_DATAFILE */);
        if (m != IntPtr.Zero)
        {
            EnumResourceNames(m, (IntPtr)RT_GROUP_ICON, (mod, t, nom, p) =>
            {
                if (trouve) return false;
                trouve = true;
                nomGroupe = ((long)nom >> 16) == 0 ? nom : (IntPtr)1; // nom numérique conservé (sinon 1)
                EnumResourceLanguages(mod, t, nom, (mm, tt, nn, l, pp) => { langue = l; return false; }, IntPtr.Zero);
                return false;
            }, IntPtr.Zero);
            EnumResourceNames(m, (IntPtr)RT_ICON, (mod, t, nom, p) => { if (((long)nom >> 16) == 0) maxIcone = Math.Max(maxIcone, (int)(long)nom); return true; }, IntPtr.Zero);
            FreeLibrary(m);
        }
        Console.WriteLine("Groupe d'icônes : " + (trouve ? "#" + (long)nomGroupe : "aucun (créé)") + ", langue " + langue + ", " + n + " images");

        // ---- écriture
        var h = BeginUpdateResource(exe, false);
        if (h == IntPtr.Zero) { Console.Error.WriteLine("Impossible d'ouvrir l'exe (" + Marshal.GetLastWin32Error() + ")"); return 2; }
        var groupe = new MemoryStream();
        var w = new BinaryWriter(groupe);
        w.Write((ushort)0); w.Write((ushort)1); w.Write((ushort)n);
        for (int i = 0; i < n; i++)
        {
            int id = maxIcone + 1 + i;
            if (!UpdateResource(h, (IntPtr)RT_ICON, (IntPtr)id, langue, images[i], (uint)images[i].Length)) { EndUpdateResource(h, true); Console.Error.WriteLine("Échec image " + i); return 3; }
            w.Write(entrees[i]);       // 12 octets : dimensions, couleurs, plans, bits, taille
            w.Write((ushort)id);      // numéro de la ressource RT_ICON
        }
        var g = groupe.ToArray();
        if (!UpdateResource(h, (IntPtr)RT_GROUP_ICON, nomGroupe, langue, g, (uint)g.Length)) { EndUpdateResource(h, true); Console.Error.WriteLine("Échec du groupe"); return 4; }
        if (!EndUpdateResource(h, false)) { Console.Error.WriteLine("Échec de l'écriture (" + Marshal.GetLastWin32Error() + ")"); return 5; }
        Console.WriteLine("Icône remplacée : " + exe);
        return 0;
    }
}
