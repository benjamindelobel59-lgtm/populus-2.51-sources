package populus
{
   import flash.display.Stage;
   import flash.geom.Rectangle;
   import flash.net.SharedObject;
   import flash.net.URLRequest;
   import flash.net.navigateToURL;
   import flash.utils.getDefinitionByName;
   import flash.utils.ByteArray;
   import flash.net.URLLoader;
   import flash.net.URLRequestMethod;
   import flash.events.Event;
   import flash.events.IOErrorEvent;
   import flash.events.SecurityErrorEvent;
   
   public class PanelWindow
   {
      
      private static var _rect:Rectangle;
      
      private static var _mini:Boolean;
      
      private static var _beforeMax:Rectangle;
      
      private static var _wv:*;
      
      private static var _stage:Stage;
      
      private static const MIN_W:int = 420;
      
      private static const MIN_H:int = 200;
      
      private static var _miniH:int = 44;
      
      private static var _k:Number = 1;
      
      public function PanelWindow()
      {
         super();
      }
      
      private static function windowSize() : Rectangle
      {
         var nw:* = undefined;
         try
         {
            nw = Object(_stage).nativeWindow;
            if(nw)
            {
               return new Rectangle(0,0,Math.max(800,nw.width - 16),Math.max(600,nw.height - 39));
            }
         }
         catch(e:Error)
         {
         }
         return new Rectangle(0,0,_stage.stageWidth,_stage.stageHeight);
      }
      
      // PADGREF 23/10 : la barre du panel (assets/ingame.js : deplacer, agrandir, reduire, fermer) et l'ordre « close » passent
      // par Edge WebView2 (<UseWebView2> de application.xml), qui n'existe qu'a partir d'Adobe AIR 50. Avec un moteur plus ancien
      // (AIR 33 du client 2.73), la page s'ouvrait dans le jeu SANS barre ni bouton fermer : on l'ouvre alors dans le navigateur.
      private static function webView2Possible() : Boolean
      {
         var NA:Class = null;
         var v:String = null;
         try
         {
            NA = getDefinitionByName("flash.desktop.NativeApplication") as Class;
            v = String(NA["nativeApplication"].runtimeVersion);
            return int(v.split(".")[0]) >= 50;
         }
         catch(e:Error)
         {
         }
         return false;
      }
      
      public static function open(param1:Stage, param2:String) : void
      {
         var stage:Stage = param1;
         var url:String = param2;
         var C:Class = null;
         try
         {
            C = getDefinitionByName("flash.media.StageWebView") as Class;
         }
         catch(e:Error)
         {
         }
         if(!stage || !C || !C["isSupported"] || !webView2Possible())
         {
            navigateToURL(new URLRequest(url),"_blank");
            return;
         }
         _stage = stage;
         if(!_wv)
         {
            _wv = new C(true);
            _wv.addEventListener("webViewMessage",onMessage);
            load();
         }
         _mini = false;
         _wv.stage = stage;
         apply();
         _wv.loadURL(url);
      }
      
      private static function onMessage(param1:*) : void
      {
         var _loc6_:Rectangle = null;
         var _loc2_:Array = String(param1.data).split("|");
         var _loc3_:Number = (Number(_loc2_[1]) || 0) * _k;
         var _loc4_:Number = (Number(_loc2_[2]) || 0) * _k;
         var _loc5_:Object = {
            "a":_loc2_[0],
            "dx":_loc3_,
            "dy":_loc4_,
            "dw":_loc3_,
            "dh":_loc4_,
            "on":_loc2_[1] == "1",
            "h":_loc4_
         };
         if(Boolean(_loc2_[0] == "w" && Number(_loc2_[1]) > 0 && !_mini) && Boolean(_wv) && Boolean(_wv.viewPort))
         {
            _k = _wv.viewPort.width / Number(_loc2_[1]);
            return;
         }
         switch(_loc5_.a)
         {
            case "move":
               _rect.x += Number(_loc5_.dx) || 0;
               _rect.y += Number(_loc5_.dy) || 0;
               apply();
               break;
            case "resize":
               _beforeMax = null;
               _rect.width += Number(_loc5_.dw) || 0;
               _rect.height += Number(_loc5_.dh) || 0;
               apply();
               break;
            case "mini":
               _mini = Boolean(_loc5_.on);
               if(_loc5_.h > 0)
               {
                  _miniH = int(_loc5_.h);
               }
               apply();
               break;
            case "max":
               if(_beforeMax)
               {
                  _rect = _beforeMax;
                  _beforeMax = null;
               }
               else
               {
                  _beforeMax = _rect.clone();
                  _loc6_ = windowSize();
                  _rect = new Rectangle(0,0,_loc6_.width,_loc6_.height);
               }
               _mini = false;
               apply();
               save();
               break;
            case "end":
               save();
               break;
            case "close":
               save();
               close();
         }
      }
      
      private static function load() : void
      {
         var so:SharedObject = null;
         var win:Rectangle = null;
         var w:Number = NaN;
         var h:Number = NaN;
         try
         {
            so = SharedObject.getLocal("populusPanel");
            if(so.data.w > 0)
            {
               _rect = new Rectangle(so.data.x,so.data.y,so.data.w,so.data.h);
            }
         }
         catch(e:Error)
         {
         }
         if(!_rect)
         {
            win = windowSize();
            w = Math.min(1300,Math.round(win.width * 0.7));
            h = Math.round(win.height * 0.8);
            _rect = new Rectangle(Math.round((win.width - w) / 2),Math.round((win.height - h) / 2),w,h);
         }
      }
      
      private static function apply() : void
      {
         if(!_wv || !_stage)
         {
            return;
         }
         var _loc1_:Rectangle = windowSize();
         _rect.width = Math.max(MIN_W,Math.min(_rect.width,_loc1_.width));
         _rect.height = Math.max(MIN_H,Math.min(_rect.height,_loc1_.height));
         _rect.x = Math.max(-_rect.width + 160,Math.min(_rect.x,_loc1_.width - 160));
         _rect.y = Math.max(0,Math.min(_rect.y,_loc1_.height - _miniH));
         _wv.viewPort = new Rectangle(Math.round(_rect.x),Math.round(_rect.y),Math.round(_rect.width),_mini ? _miniH : Math.round(_rect.height));
      }
      
      // Mystic : « Signaler un bug » : envoie la fin (2 Mo max) du fichier de log le plus récent du client
      // (%APPDATA%\Dofus-x64\logs ou dossier logs du stockage de l'application) au formulaire du panel.
      public static function envoyerLog(param1:String) : void
      {
         var url:String = param1;
         var F:Class = null;
         var FS:Class = null;
         var dossiers:Array = null;
         var d:* = undefined;
         var f:* = undefined;
         var recent:* = null;
         var fs:* = null;
         var octets:ByteArray = null;
         var taille:Number = NaN;
         var req:URLRequest = null;
         var ld:URLLoader = null;
         try
         {
            F = getDefinitionByName("flash.filesystem.File") as Class;
            FS = getDefinitionByName("flash.filesystem.FileStream") as Class;
            dossiers = [];
            try
            {
               dossiers.push(F["applicationStorageDirectory"].resolvePath("logs"));
               dossiers.push(F["applicationStorageDirectory"].parent.resolvePath("logs"));
            }
            catch(e1:Error)
            {
            }
            try
            {
               dossiers.push(F["userDirectory"].resolvePath("AppData/Roaming/Dofus-x64/logs"));
               dossiers.push(F["userDirectory"].resolvePath("AppData/Roaming/Dofus/logs"));
            }
            catch(e2:Error)
            {
            }
            for each(d in dossiers)
            {
               if(d && d.exists && d.isDirectory)
               {
                  for each(f in d.getDirectoryListing())
                  {
                     if(!f.isDirectory && String(f.name).indexOf(".log") > 0 && (!recent || f.modificationDate.time > recent.modificationDate.time))
                     {
                        recent = f;
                     }
                  }
               }
            }
            octets = new ByteArray();
            if(recent)
            {
               fs = new FS();
               fs.open(recent,"read");
               taille = Number(recent.size);
               if(taille > 2000000)
               {
                  fs.position = taille - 2000000;
               }
               fs.readBytes(octets,0,fs.bytesAvailable);
               fs.close();
            }
            else
            {
               octets.writeUTFBytes("(aucun fichier de log trouvé sur ce PC)");
            }
            req = new URLRequest(url);
            req.method = URLRequestMethod.POST;
            req.contentType = "text/plain; charset=utf-8";
            req.data = octets;
            ld = new URLLoader();
            ld.addEventListener(Event.COMPLETE,ignorer);
            ld.addEventListener(IOErrorEvent.IO_ERROR,ignorer);
            ld.addEventListener(SecurityErrorEvent.SECURITY_ERROR,ignorer);
            ld.load(req);
         }
         catch(e:Error)
         {
         }
      }

      private static function ignorer(param1:* = null) : void
      {
      }

      public static function close() : void
      {
         if(_wv)
         {
            _wv.stage = null;
         }
      }
      
      private static function save() : void
      {
         var so:SharedObject = null;
         try
         {
            so = SharedObject.getLocal("populusPanel");
            so.data.x = _rect.x;
            so.data.y = _rect.y;
            so.data.w = _rect.width;
            so.data.h = _rect.height;
            so.flush();
         }
         catch(e:Error)
         {
         }
      }
   }
}

