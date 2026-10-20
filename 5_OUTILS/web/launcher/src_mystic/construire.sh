# PADGREF 20/10 : compile le launcher (bash). Usage : bash src_mystic/construire.sh [sortie.exe]  (défaut : %TEMP%/lb/PadgrefLauncher.exe)
cd "/c/Users/Beny/Desktop/DOFUS NEKFEU JTE/_OUTILS/web/launcher"
mkdir -p "$TEMP/lb"
R="C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8"; refs=()
for a in mscorlib System System.Core System.Xml PresentationCore PresentationFramework WindowsBase System.Xaml System.Drawing System.Windows.Forms System.Web.Extensions System.Security; do refs+=("-r:$R/$a.dll"); done
res=()
for f in p_bienvenue.jpg p_logo.png p_zaap.png p_icone_or.png p_icone_bleu.png p_icone_rouge.png p_icone_gris.png p_ogrine.png p_groupe.png p_actu1.jpg p_actu2.jpg; do res+=("-resource:src_mystic/ressources/$f,$f"); done
"/c/Program Files (x86)/Microsoft Visual Studio/2022/BuildTools/MSBuild/Current/Bin/Roslyn/csc.exe" -nologo -target:winexe -optimize+ -out:"${1:-$TEMP/lb/PadgrefLauncher.exe}" -win32icon:src_mystic/ressources/padgref.ico -nostdlib "${refs[@]}" "${res[@]}" src_mystic/*.cs
