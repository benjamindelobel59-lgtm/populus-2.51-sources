// PADGREF 21/10 (demandes utilisateur) : icônes officielles du jeu pour les tags des comptes et les icônes de dossiers
// (symboles de classes, épées…), lues dans le jeu installé (comme les têtes de classe) ; et style des menus du clic droit
// (avant : menu Windows standard, « moche »).

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static MysticLauncher.Theme;

namespace MysticLauncher
{
    static class Icones
    {
        public class Icone
        {
            public string Cle, Nom, Chemin, Groupe;
        }

        static List<Icone> s_catalogue;
        static readonly Dictionary<string, ImageSource> s_images = new Dictionary<string, ImageSource>();

        /// <summary>Icônes proposées : classes (gfx/classes/symbol_N.png) et épées / divers (thème darkStone du client).</summary>
        public static List<Icone> Catalogue(string appDir)
        {
            if (s_catalogue != null) return s_catalogue;
            var l = new List<Icone>();
            for (var i = 1; i <= 18; i++)
                l.Add(new Icone { Cle = "classe_" + i, Nom = Outils.Classe(i), Groupe = "Classes", Chemin = Path.Combine("content", "gfx", "classes", "symbol_" + i + ".png") });
            var t = Path.Combine("content", "themes", "darkStone", "texture");
            l.Add(new Icone { Cle = "epees", Nom = "Épées croisées", Groupe = "Épées", Chemin = Path.Combine(t, "icon_crossed_swords.png") });
            l.Add(new Icone { Cle = "epees_claires", Nom = "Épées croisées (claires)", Groupe = "Épées", Chemin = Path.Combine(t, "icon_crossed_swords_light.png") });
            l.Add(new Icone { Cle = "epee", Nom = "Épée", Groupe = "Épées", Chemin = Path.Combine(t, "sword_icon.png") });
            l.Add(new Icone { Cle = "epee_rouge", Nom = "Épée rouge", Groupe = "Épées", Chemin = Path.Combine(t, "red_sword_icon.png") });
            l.Add(new Icone { Cle = "pvp", Nom = "PvP", Groupe = "Épées", Chemin = Path.Combine(t, "hud", "icon_swords_pvp.png") });
            l.Add(new Icone { Cle = "trophee", Nom = "Trophée", Groupe = "Divers", Chemin = Path.Combine(t, "icon_trophy.png") });
            l.Add(new Icone { Cle = "ogrine", Nom = "Ogrine", Groupe = "Divers", Chemin = Path.Combine(t, "icon_ogrine.png") });
            s_catalogue = l.Where(x => appDir != null && File.Exists(Path.Combine(appDir, x.Chemin))).ToList();
            return s_catalogue;
        }

        public static Icone Trouver(string appDir, string cle) => cle == null ? null : Catalogue(appDir).FirstOrDefault(x => x.Cle == cle);

        public static ImageSource Image(string appDir, string cle)
        {
            if (cle == null) return null;
            if (s_images.TryGetValue(cle, out var deja)) return deja;
            ImageSource img = null;
            var ic = Trouver(appDir, cle);
            if (ic != null)
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(Path.Combine(appDir, ic.Chemin));
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();
                    img = bmp;
                }
                catch { img = null; }
            }
            s_images[cle] = img;
            return img;
        }

        public static FrameworkElement Vue(string appDir, string cle, double taille)
        {
            var src = Image(appDir, cle);
            if (src == null) return null;
            var i = new System.Windows.Controls.Image { Source = src, Width = taille, Height = taille, Stretch = Stretch.Uniform, SnapsToDevicePixels = true };
            RenderOptions.SetBitmapScalingMode(i, BitmapScalingMode.HighQuality);
            var ic = Trouver(appDir, cle);
            if (ic != null) i.ToolTip = ic.Nom;
            return i;
        }

        // ---------------------------------------------------------------- style des menus (clic droit)

        static ResourceDictionary s_styles;

        static string Hex(Color c) => "#" + c.A.ToString("X2") + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");

        /// <summary>Styles des ContextMenu / MenuItem / Separator : cadre arrondi sombre, survol coloré, colonne d'icônes, coche, flèche de sous-menu.</summary>
        public static ResourceDictionary Styles()
        {
            if (s_styles != null) return s_styles;
            var x = @"
<ResourceDictionary xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Style TargetType='ContextMenu'>
    <Setter Property='OverridesDefaultStyle' Value='True'/>
    <Setter Property='SnapsToDevicePixels' Value='True'/>
    <Setter Property='HasDropShadow' Value='False'/>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='ContextMenu'>
          <Border Background='{FOND}' BorderBrush='{LIGNE}' BorderThickness='1' CornerRadius='12' Padding='6' Margin='0,0,10,10'>
            <Border.Effect><DropShadowEffect Color='Black' BlurRadius='18' ShadowDepth='5' Direction='270' Opacity='0.55'/></Border.Effect>
            <StackPanel IsItemsHost='True' KeyboardNavigation.DirectionalNavigation='Cycle' MinWidth='190'/>
          </Border>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>
  <Style TargetType='Separator'>
    <Setter Property='OverridesDefaultStyle' Value='True'/>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='Separator'>
          <Border Height='1' Background='{LIGNE}' Margin='8,5,8,5'/>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>
  <Style TargetType='MenuItem'>
    <Setter Property='OverridesDefaultStyle' Value='True'/>
    <Setter Property='Foreground' Value='{TEXTE}'/>
    <Setter Property='FontSize' Value='13'/>
    <Setter Property='Cursor' Value='Hand'/>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='MenuItem'>
          <Grid>
            <Border x:Name='fond' CornerRadius='8' Background='Transparent' Padding='8,6,10,6'>
              <Grid>
                <Grid.ColumnDefinitions>
                  <ColumnDefinition Width='26'/>
                  <ColumnDefinition Width='*'/>
                  <ColumnDefinition Width='Auto'/>
                </Grid.ColumnDefinitions>
                <ContentPresenter x:Name='icone' ContentSource='Icon' VerticalAlignment='Center' HorizontalAlignment='Center' Width='18' Height='18'/>
                <Path x:Name='coche' Grid.Column='2' Margin='8,0,0,0' Visibility='Collapsed' Data='M2,7 L6,11 L14,3' Stroke='{ACCENT2}' StrokeThickness='2.2' VerticalAlignment='Center' HorizontalAlignment='Center'/>
                <ContentPresenter Grid.Column='1' ContentSource='Header' RecognizesAccessKey='False' VerticalAlignment='Center' Margin='6,0,12,0'/>
                <Path x:Name='fleche' Grid.Column='2' Visibility='Collapsed' Data='M0,0 L4,4 L0,8' Stroke='{DOUX}' StrokeThickness='1.6' VerticalAlignment='Center'/>
              </Grid>
            </Border>
            <Popup x:Name='PART_Popup' Placement='Right' HorizontalOffset='4' IsOpen='{Binding IsSubmenuOpen, RelativeSource={RelativeSource TemplatedParent}}' AllowsTransparency='True' Focusable='False' PopupAnimation='Fade'>
              <Border Background='{FOND}' BorderBrush='{LIGNE}' BorderThickness='1' CornerRadius='12' Padding='6' Margin='0,0,10,10'>
                <Border.Effect><DropShadowEffect Color='Black' BlurRadius='18' ShadowDepth='5' Direction='270' Opacity='0.55'/></Border.Effect>
                <ScrollViewer VerticalScrollBarVisibility='Auto' MaxHeight='460'>
                  <StackPanel IsItemsHost='True' KeyboardNavigation.DirectionalNavigation='Cycle' MinWidth='180'/>
                </ScrollViewer>
              </Border>
            </Popup>
          </Grid>
          <ControlTemplate.Triggers>
            <Trigger Property='IsHighlighted' Value='True'>
              <Setter TargetName='fond' Property='Background' Value='{SURVOL}'/>
            </Trigger>
            <Trigger Property='IsChecked' Value='True'>
              <Setter TargetName='coche' Property='Visibility' Value='Visible'/>
              <Setter TargetName='fond' Property='Background' Value='{COCHE}'/>
            </Trigger>
            <Trigger Property='HasItems' Value='True'>
              <Setter TargetName='fleche' Property='Visibility' Value='Visible'/>
            </Trigger>
            <Trigger Property='IsEnabled' Value='False'>
              <Setter Property='Opacity' Value='0.45'/>
            </Trigger>
          </ControlTemplate.Triggers>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>
</ResourceDictionary>";
            x = x.Replace("{FOND}", Hex(Fond2)).Replace("{LIGNE}", Hex(Ligne)).Replace("{TEXTE}", Hex(Texte)).Replace("{DOUX}", Hex(Doux))
                 .Replace("{ACCENT2}", Hex(Accent2)).Replace("{SURVOL}", Hex(A(0x38, Accent))).Replace("{COCHE}", Hex(A(0x1E, Accent2)));
            s_styles = (ResourceDictionary)XamlReader.Parse(x);
            return s_styles;
        }
    }
}
