package ui
{
   import com.ankamagames.berilia.api.UiApi;
   import com.ankamagames.berilia.components.Texture;
   import com.ankamagames.berilia.types.graphic.ButtonContainer;
   import com.ankamagames.berilia.types.graphic.GraphicContainer;
   import com.ankamagames.berilia.types.graphic.UiRootContainer;
   import com.ankamagames.dofus.datacenter.misc.OptionalFeature;
   import com.ankamagames.dofus.uiApi.ConfigApi;
   import com.ankamagames.dofus.uiApi.DataApi;
   import com.ankamagames.dofus.uiApi.SecurityApi;
   import com.ankamagames.dofus.uiApi.SystemApi;
   import com.ankamagames.dofus.uiApi.UiTutoApi;
   import com.ankamagames.dofusModuleLibrary.enum.SoundEnum;
   import d2enums.ComponentHookList;
   import d2enums.ShortcutHookListEnum;
   import enum.WebTabEnum;
   import flash.external.ExternalInterface;
   import flash.filters.BlurFilter;
   import flash.geom.Point;
   
   public class WebBase
   {
      
      public static var currentTabUi:String;
      
      public static var isShopAvailable:Boolean = false;
      
      public static var isVetRewardsAvailable:Boolean = false;
      
      public static var isLibraryAvailable:Boolean = false;
      
      public static const blurFilter:BlurFilter = new BlurFilter(10,10);
      
      public var sysApi:SystemApi;
      
      public var uiApi:UiApi;
      
      public var configApi:ConfigApi;
      
      public var dataApi:DataApi;
      
      public var securityApi:SecurityApi;
      
      public var hintsApi:UiTutoApi;
      
      public var uiCtr:GraphicContainer;
      
      public var btn_close:ButtonContainer;
      
      public var btn_tabShop:ButtonContainer;
      
      public var btn_tabOgrines:ButtonContainer;
      
      public var btn_tabVeteranRewards:ButtonContainer;
      
      public var btn_tabLibrary:ButtonContainer;
      
      public var btn_help:ButtonContainer;
      
      public var tx_line:Texture;
      
      public var ctr_linemask:GraphicContainer;
      
      public function WebBase()
      {
         super();
      }
      
      public function main(oParams:* = null) : void
      {
         var lastTab:String = null;
         this.uiApi.me().restoreSnapshotAfterLoading = false;
         this.tx_line.mask = this.ctr_linemask;
         this.uiApi.addShortcutHook("closeUi",this.onShortcut);
         this.uiApi.addComponentHook(this.btn_tabOgrines,"onRelease");
         this.uiApi.addComponentHook(this.btn_tabVeteranRewards,"onRelease");
         this.uiApi.addComponentHook(this.btn_tabShop,"onRelease");
         this.uiApi.addComponentHook(this.btn_help,ComponentHookList.ON_RELEASE);
         this.btn_close.soundId = SoundEnum.CANCEL_BUTTON;
         var feature:OptionalFeature = this.dataApi.getOptionalFeatureByKeyword("web.shopInClient");
         if(Boolean(feature) && this.configApi.isOptionalFeatureActive(feature.id))
         {
            isShopAvailable = true;
         }
         isShopAvailable = true;
         currentTabUi = null;
         this.btn_tabOgrines.visible = false;
         this.btn_tabVeteranRewards.visible = false;
         if(isShopAvailable)
         {
            this.btn_tabShop.visible = true;
         }

         if(isLibraryAvailable)
         {
            this.btn_tabLibrary.visible = true;
         }
         if(Boolean(oParams) && Boolean(oParams is Array) && Boolean(oParams[0]))
         {
            this.openTab(oParams[0],oParams[1]);
         }
         else
         {
            lastTab = WebTabEnum.SHOP_TAB;
            if((this.sysApi.isSteamEmbed() || this.sysApi.getCurrentServer().gameTypeId == 5) && lastTab == WebTabEnum.BAK_TAB)
            {
               lastTab = WebTabEnum.SHOP_TAB;
            }
            if(!isShopAvailable && lastTab == WebTabEnum.SHOP_TAB || !isVetRewardsAvailable && lastTab == WebTabEnum.VET_RWDS_TAB)
            {
               lastTab = WebTabEnum.BAK_TAB;
            }
            this.openTab(lastTab);
         }
      }
      
      public function unload() : void
      {
         if(currentTabUi == WebTabEnum.BAK_TAB)
         {
            this.closeWebModalWindow();
         }
         if(currentTabUi)
         {
            this.uiApi.unloadUi(currentTabUi);
         }
         if(this.uiApi.getUi("secureMode"))
         {
            this.uiApi.unloadUi("secureMode");
         }
         currentTabUi = null;
      }
      
      public function onShortcut(s:String) : Boolean
      {
         switch(s)
         {
            case ShortcutHookListEnum.CLOSE_UI:
               this.uiApi.unloadUi(this.uiApi.me().name);
               return true;
            default:
               return false;
         }
      }
      
      public function onRelease(target:Object) : void
      {
         switch(target)
         {
            case this.btn_tabShop:
               this.openTab(WebTabEnum.SHOP_TAB);
               break;
            case this.btn_tabOgrines:
               this.openTab(WebTabEnum.BAK_TAB);
               break;
            case this.btn_tabVeteranRewards:
               this.openTab(WebTabEnum.VET_RWDS_TAB);
               break;
            case this.btn_tabLibrary:
               this.openTab(WebTabEnum.LIBRARY_TAB);
               break;
            case this.btn_close:
               this.uiApi.unloadUi(this.uiApi.me().name);
               break;
            case this.btn_help:
               this.uiApi.me().childUiRoot.uiClass.showTabHints();
         }
      }
      
      public function openTab(uiName:String = null, uiParams:Object = null) : void
      {
         var shieldUp:Boolean = false;
         if(uiName == WebTabEnum.BAK_TAB || uiName == WebTabEnum.VET_RWDS_TAB)
         {
            uiName = WebTabEnum.SHOP_TAB;
         }
         if(!shieldUp && (uiName == WebTabEnum.BAK_TAB || !this.sysApi.isSteamEmbed() && uiName == WebTabEnum.SHOP_TAB))
         {
            if(!this.uiApi.getUi("secureMode"))
            {
               if(currentTabUi)
               {
                  this.uiApi.getUi(currentTabUi).filters = [blurFilter];
                  this.uiApi.getUi(currentTabUi).disabled = true;
               }
               this.uiApi.loadUiInside("Ankama_Common::secureMode",this.uiCtr,"secureMode",{
                  "reboot":false,
                  "callBackOnSecured":this.onSecureConfirmed,
                  "callBackOnSecuredParams":[uiName,uiParams],
                  "callBackOnCancelled":this.onSecureRequestCancelled,
                  "offset":new Point(-80,-100),
                  "step":100
               }) as UiRootContainer;
               this.updateSelectedTab(uiName);
            }
            return;
         }
         if(uiName == currentTabUi)
         {
            return;
         }
         if(this.uiApi.getUi("secureMode"))
         {
            this.uiApi.unloadUi("secureMode");
         }
         if(currentTabUi == WebTabEnum.BAK_TAB)
         {
            this.closeWebModalWindow();
         }
         if(currentTabUi)
         {
            this.uiApi.unloadUi(currentTabUi);
         }
         currentTabUi = uiName;
         this.sysApi.setData("shopLastOpenedTab",currentTabUi);
         this.updateSelectedTab();
         this.uiApi.loadUiInside(currentTabUi,this.uiCtr,currentTabUi,uiParams) as UiRootContainer;
      }
      
      private function onSecureConfirmed(params:Array) : void
      {
         this.openTab(params[0],params[1]);
      }
      
      private function onSecureRequestCancelled() : void
      {
         if(currentTabUi)
         {
            this.uiApi.getUi(currentTabUi).filters = null;
            this.uiApi.getUi(currentTabUi).disabled = false;
            this.updateSelectedTab();
         }
      }
      
      private function updateSelectedTab(uiName:String = "") : void
      {
         if(!uiName)
         {
            uiName = currentTabUi;
         }
         switch(uiName)
         {
            case WebTabEnum.SHOP_TAB:
               this.btn_tabShop.selected = true;
               break;
            case WebTabEnum.BAK_TAB:
               this.btn_tabOgrines.selected = true;
               break;
            case WebTabEnum.VET_RWDS_TAB:
               this.btn_tabVeteranRewards.selected = true;
               break;
            case WebTabEnum.LIBRARY_TAB:
               this.btn_tabLibrary.selected = true;
         }
      }
      
      private function closeWebModalWindow() : void
      {
         if(ExternalInterface.available)
         {
            ExternalInterface.call("closeModal");
         }
      }
   }
}

