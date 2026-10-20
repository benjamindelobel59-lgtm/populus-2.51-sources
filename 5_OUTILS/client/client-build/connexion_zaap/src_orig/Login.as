package ui
{
   import com.ankamagames.berilia.api.UiApi;
   import com.ankamagames.berilia.components.ComboBox;
   import com.ankamagames.berilia.components.Grid;
   import com.ankamagames.berilia.components.Input;
   import com.ankamagames.berilia.components.InputComboBox;
   import com.ankamagames.berilia.components.Label;
   import com.ankamagames.berilia.components.WebBrowser;
   import com.ankamagames.berilia.types.graphic.ButtonContainer;
   import com.ankamagames.berilia.types.graphic.GraphicContainer;
   import com.ankamagames.berilia.types.graphic.StateContainer;
   import com.ankamagames.dofus.uiApi.ConfigApi;
   import com.ankamagames.dofus.uiApi.ConnectionApi;
   import com.ankamagames.dofus.uiApi.DataApi;
   import com.ankamagames.dofus.uiApi.SoundApi;
   import com.ankamagames.dofus.uiApi.SystemApi;
   import com.ankamagames.dofus.uiApi.TooltipApi;
   import com.ankamagames.dofus.uiApi.UtilApi;
   import com.ankamagames.dofusModuleLibrary.enum.SoundTypeEnum;
   import com.ankamagames.dofusModuleLibrary.enum.components.GridItemSelectMethodEnum;
   import d2actions.*;
   import d2enums.BuildTypeEnum;
   import d2enums.ComponentHookList;
   import d2enums.WebLocationEnum;
   import d2hooks.*;
   import flash.events.Event;
   import flash.events.IOErrorEvent;
   import flash.net.URLLoader;
   import flash.net.URLRequest;
   import flash.ui.Keyboard;
   import flash.utils.*;
   
   public class Login
   {
      
      private static const GAMEMODE_NONE:uint = 0;
      
      private static const GAMEMODE_LOG_IN:uint = 1;
      
      private static const GAMEMODE_PLAY_AS_GUEST:uint = 2;
      
      private static const GAMEMODE_LOG_IN_OAUTH:uint = 3;
      
      private static const GAMEMODE_LOG_IN_STEAM:uint = 4;
      
      private static const GAMEMODE_LOG_IN_ZAAP:uint = 5;
      
      private static const LOCAL_NEWS_URL:String = "http://utils.dofus.lan/news/";
      
      private static const RELEASE_NEWS_URL:String = "http://dl.ak.ankama.com/games/dofus2/news/";
      
      public var dataApi:DataApi;
      
      public var sysApi:SystemApi;
      
      public var configApi:ConfigApi;
      
      public var uiApi:UiApi;
      
      public var soundApi:SoundApi;
      
      public var connectionApi:ConnectionApi;
      
      public var utilApi:UtilApi;
      
      public var tooltipApi:TooltipApi;
      
      [Module(name="Ankama_Common")]
      public var modCommon:Object;
      
      private var _guestModeAvailable:Boolean = false;
      
      private var _oauthLoginAvailable:Boolean = false;
      
      private var _zaapLoginAvailable:Boolean = false;
      
      private var _currentMode:uint = 0;
      
      private var _aPorts:Array;
      
      private var _aPortsName:Array;
      
      private var _componentsList:Dictionary = new Dictionary(true);
      
      private var _previousFocus:Input;
      
      private var _ankamaAuthUrl:String;
      
      private var _ankamaAuthBaseUrl:String;
      
      private var _domain:int = -1;
      
      private var _mustDisableConnectionButton:Boolean = false;
      
      private var _timeoutId:uint;
      
      private var _newsLoader:URLLoader;
      
      public var ctr_welcome:GraphicContainer;
      
      public var ctr_tabs:GraphicContainer;
      
      public var btn_registeredTab:ButtonContainer;
      
      public var btn_guestTab:ButtonContainer;
      
      public var ctr_center:GraphicContainer;
      
      public var ctr_links:GraphicContainer;
      
      public var ctr_inputs:StateContainer;
      
      public var ctr_login:GraphicContainer;
      
      public var ctr_pass:GraphicContainer;
      
      public var cbx_login:InputComboBox;
      
      public var input_pass:Input;
      
      public var lbl_login:Label;
      
      public var lbl_pass:Label;
      
      public var lbl_capsLock:Label;
      
      public var ctr_capsLockMsg:GraphicContainer;
      
      public var btn_passForgotten:ButtonContainer;
      
      public var btn_createAccount:ButtonContainer;
      
      public var ctr_guestMode:GraphicContainer;
      
      public var lbl_guestModeInfo:Label;
      
      public var btn_play:ButtonContainer;
      
      public var btn_options:ButtonContainer;
      
      public var ctr_options:GraphicContainer;
      
      public var btn_rememberLogin:ButtonContainer;
      
      public var cb_connectionType:ComboBox;
      
      public var cb_socket:ComboBox;
      
      public var ctr_gifts:GraphicContainer;
      
      public var btn_members:ButtonContainer;
      
      public var btn_lowa:ButtonContainer;
      
      public var gd_shop:Grid;
      
      public var ctr_oauthServices:GraphicContainer;
      
      public var btn_login_ankama:ButtonContainer;
      
      public var btn_login_facebook:ButtonContainer;
      
      public var btn_login_twitter:ButtonContainer;
      
      public var btn_login_google:ButtonContainer;
      
      public var ctr_steam:GraphicContainer;
      
      public var btn_login_steam:ButtonContainer;
      
      public var ctr_zaap:GraphicContainer;
      
      public var btn_login_zaap:ButtonContainer;
      
      public var ctr_webLogin:GraphicContainer;
      
      public var wb_webLogin:WebBrowser;
      
      public var btn_webLoginClose:ButtonContainer;
      
      public var btn_oauth:ButtonContainer;
      
      public function Login()
      {
         super();
      }
      
      public static function getJsonFile(type:uint) : String
      {
         switch(type)
         {
            case BuildTypeEnum.RELEASE:
               return "release";
            case BuildTypeEnum.BETA:
               return "beta";
            case BuildTypeEnum.TESTING:
               return "testing";
            case BuildTypeEnum.INTERNAL:
               return "local";
            case BuildTypeEnum.DEBUG:
               return "debug";
            default:
               return null;
         }
      }
      
      public function main(params:Array) : void
      {
         var popup:String = null;
         var porc:String = null;
         var serverport:uint = 0;
         var loginMustBeSaved:int = 0;
         var lastLogins:Array = null;
         var deprecatedLogin:String = null;
         var logins:Array = null;
         if(params.length > 0)
         {
            popup = params[0];
         }
         if(params.length > 1 && !this.uiApi.me().uiModule.mainClass.unlocked)
         {
            this._mustDisableConnectionButton = params[1];
         }
         var initialUiMode:uint = this.sysApi.getSetData("loginUiMode",GAMEMODE_NONE);
         if(this.sysApi.isUsingZaapLogin())
         {
            this._zaapLoginAvailable = true;
         }
         this.btn_play.soundId = "-1";
         this.soundApi.playIntroMusic();
         this.sysApi.addHook(NicknameRegistration,this.onNicknameRegistration);
         this.sysApi.addHook(SubscribersList,this.onSubscribersList);
         this.sysApi.addHook(UiUnloaded,this.onUiUnloaded);
         this.sysApi.addHook(SelectedServerFailed,this.onSelectedServerFailed);
         this.sysApi.addHook(KeyUp,this.onKeyUp);
         this.sysApi.addHook(FontActiveTypeChanged,this.onFontChanged);
         this.uiApi.addShortcutHook("validUi",this.onShortcut);
         this.uiApi.addComponentHook(this.btn_rememberLogin,ComponentHookList.ON_ROLL_OVER);
         this.uiApi.addComponentHook(this.btn_rememberLogin,ComponentHookList.ON_ROLL_OUT);
         this.uiApi.addComponentHook(this.btn_oauth,ComponentHookList.ON_ROLL_OVER);
         this.uiApi.addComponentHook(this.btn_oauth,ComponentHookList.ON_ROLL_OUT);
         this.uiApi.addComponentHook(this.btn_rememberLogin,ComponentHookList.ON_RELEASE);
         this.uiApi.addComponentHook(this.cb_connectionType,ComponentHookList.ON_SELECT_ITEM);
         this.uiApi.addComponentHook(this.cb_socket,ComponentHookList.ON_SELECT_ITEM);
         this.uiApi.addComponentHook(this.input_pass,"onChange");
         if(!this.sysApi.isSteamEmbed())
         {
            this.sysApi.sendAction(new SubscribersGiftListRequest());
         }
         this.ctr_gifts.visible = false;
         this.ctr_tabs.visible = this._guestModeAvailable;
         this.ctr_welcome.visible = !this._guestModeAvailable;
         if(!this._oauthLoginAvailable)
         {
            this.btn_oauth.visible = false;
         }
         this.uiApi.addComponentHook(this.cbx_login,ComponentHookList.ON_SELECT_ITEM);
         this.uiApi.addComponentHook(this.cbx_login.input,"onChange");
         this.ctr_options.visible = false;
         if(this.connectionApi.hasGuestAccount())
         {
            this.lbl_guestModeInfo.text = this.uiApi.getText("ui.guest.guestAccountExisting");
         }
         else
         {
            this.lbl_guestModeInfo.text = this.uiApi.getText("ui.guest.guestModeInfo");
         }
         this.uiApi.setRadioGroupSelectedItem("tabHGroup",this.btn_registeredTab,this.uiApi.me());
         this.btn_registeredTab.selected = true;
         this.cbx_login.input.tabEnabled = true;
         this.input_pass.tabEnabled = true;
         var cachePortIsStillOk:Boolean = false;
         var cachePort:int = int(this.sysApi.getPort());
         this._aPorts = new Array();
         this._aPortsName = new Array();
         var ports:String = this.sysApi.getConfigKey("connection.port");
         for each(porc in ports.split(","))
         {
            this._aPorts.push(int(porc));
            this._aPortsName.push("" + porc);
            if(cachePort == int(porc))
            {
               cachePortIsStillOk = true;
            }
         }
         this.cb_socket.dataProvider = this._aPortsName;
         if(Boolean(cachePort) && cachePortIsStillOk)
         {
            serverport = uint(cachePort);
         }
         else
         {
            serverport = uint(this._aPorts[0]);
            this.sysApi.setPort(this._aPorts[0]);
         }
         this.cb_socket.value = this._aPortsName[this._aPorts.indexOf(serverport)];
         this.cb_connectionType.dataProvider = [{
            "label":this.uiApi.getText("ui.connection.connectionToServerChoice"),
            "type":0
         },{
            "label":this.uiApi.getText("ui.connection.connectionToCharacterChoice"),
            "type":1
         },{
            "label":this.uiApi.getText("ui.connection.connectionDirectAccess"),
            "type":2
         }];
         var autoConnectType:uint = this.configApi.getConfigProperty("dofus","autoConnectType");
         this.cb_connectionType.value = this.cb_connectionType.dataProvider[autoConnectType];
         if(this._oauthLoginAvailable)
         {
            this.wb_webLogin = this.uiApi.createComponent("WebBrowser") as WebBrowser;
            this.wb_webLogin.x = 12;
            this.wb_webLogin.y = 10;
            this.wb_webLogin.width = 1179;
            this.wb_webLogin.height = 660;
            this.wb_webLogin.displayScrollBar = false;
            this.wb_webLogin.finalize();
            this.uiApi.addChildAt(this.ctr_webLogin,this.wb_webLogin,0);
            this.uiApi.addComponentHook(this.wb_webLogin,"onBrowserSessionTimeout");
            this.uiApi.addComponentHook(this.wb_webLogin,"onBrowserDomReady");
            this._domain = WebLocationEnum.WEB_LOCATION_WEB_AUTHENTIFICATION;
         }
         this.cbx_login.input.restrict = "A-Za-z0-9\\-\\|.@_[]";
         if(this.sysApi.isEventMode())
         {
            this.uiApi.setFullScreen(true,true);
            this.cbx_login.input.text = this.uiApi.getText("ui.connection.eventModeLogin");
            this.input_pass.text = "**********";
            this.cbx_login.disabled = true;
            this.input_pass.disabled = true;
            this.ctr_inputs.state = "DISABLED";
            this.btn_rememberLogin.disabled = true;
            this.btn_rememberLogin.mouseEnabled = false;
            this.cb_connectionType.disabled = true;
         }
         else
         {
            if(this.sysApi.getConfigKey("boo") == "1" && this.sysApi.getBuildType() > 1)
            {
               this.input_pass.text = this.sysApi.getData("LastPassword") ? this.sysApi.getData("LastPassword") : "";
            }
            else
            {
               this.lbl_pass.text = this.uiApi.getText("ui.login.password");
            }
            loginMustBeSaved = this.sysApi.getData("saveLogin");
            if(loginMustBeSaved == 0)
            {
               Connection.loginMustBeSaved = 1;
               this.btn_rememberLogin.selected = true;
               this.sysApi.setData("saveLogin",1);
            }
            else
            {
               Connection.loginMustBeSaved = loginMustBeSaved;
               this.btn_rememberLogin.selected = loginMustBeSaved == 1;
            }
            lastLogins = this.sysApi.getData("LastLogins");
            if(Boolean(lastLogins) && lastLogins.length > 0)
            {
               if(this.sysApi.getBuildType() != BuildTypeEnum.DEBUG && lastLogins.length >= this.sysApi.getNumberOfClients())
               {
                  this.cbx_login.input.text = lastLogins[this.sysApi.getClientId() - 1];
               }
               else
               {
                  this.cbx_login.input.text = lastLogins[0];
               }
               this.cbx_login.dataProvider = lastLogins;
               this.input_pass.focus();
            }
            else
            {
               deprecatedLogin = this.sysApi.getData("LastLogin");
               if(Boolean(deprecatedLogin) && deprecatedLogin.length > 0)
               {
                  this.sysApi.setData("LastLogin","");
                  logins = new Array();
                  logins.push(deprecatedLogin);
                  this.sysApi.setData("LastLogins",logins);
                  this.sysApi.setData("saveLogin",1);
                  Connection.loginMustBeSaved = 1;
                  this.btn_rememberLogin.selected = true;
                  this.cbx_login.input.text = deprecatedLogin;
                  this.cbx_login.dataProvider = logins;
                  this.input_pass.focus();
               }
               else
               {
                  this.lbl_login.text = this.uiApi.getText("ui.login.username");
                  this.cbx_login.input.focus();
               }
            }
         }
         if(popup == "unexpectedSocketClosure")
         {
            this.modCommon.openPopup(this.uiApi.getText("ui.popup.unexpectedSocketClosure"),this.uiApi.getText("ui.popup.unexpectedSocketClosure.text"),[this.uiApi.getText("ui.common.ok")]);
         }
         this.lbl_capsLock.multiline = false;
         this.lbl_capsLock.wordWrap = false;
         this.lbl_capsLock.fullWidthAndHeight();
         this.ctr_capsLockMsg.width = this.lbl_capsLock.textfield.width + 12;
         if(!Keyboard.capsLock || this.sysApi.getOs() == "Mac OS")
         {
            this.lbl_capsLock.visible = false;
            this.ctr_capsLockMsg.visible = false;
         }
         this.sysApi.dispatchHook(QualitySelectionRequired);
         if(this._zaapLoginAvailable)
         {
            initialUiMode = GAMEMODE_LOG_IN_ZAAP;
         }
         else if(this.sysApi.isSteamEmbed())
         {
            initialUiMode = GAMEMODE_LOG_IN_STEAM;
         }
         else if(initialUiMode == GAMEMODE_NONE || initialUiMode == GAMEMODE_LOG_IN_STEAM || initialUiMode == GAMEMODE_LOG_IN_ZAAP)
         {
            if(Boolean(this.cbx_login.dataProvider) && Boolean(this.cbx_login.dataProvider.length))
            {
               initialUiMode = GAMEMODE_LOG_IN;
            }
            else if(this._guestModeAvailable && this.connectionApi.hasGuestAccount())
            {
               initialUiMode = GAMEMODE_PLAY_AS_GUEST;
            }
            else if(this._oauthLoginAvailable)
            {
               initialUiMode = GAMEMODE_LOG_IN_OAUTH;
            }
            else
            {
               initialUiMode = GAMEMODE_LOG_IN;
            }
         }
         else if(!this._guestModeAvailable && initialUiMode == GAMEMODE_PLAY_AS_GUEST || !this._oauthLoginAvailable && initialUiMode == GAMEMODE_LOG_IN_OAUTH || !this._zaapLoginAvailable && initialUiMode == GAMEMODE_LOG_IN_ZAAP)
         {
            initialUiMode = GAMEMODE_LOG_IN;
         }
         this._timeoutId = setTimeout(this.switchUiMode,1,initialUiMode);
         var updateInformationUi:Object = this.uiApi.getUi("updateInformation");
         if(updateInformationUi == null)
         {
            this.loadNews();
         }
         else
         {
            this.sysApi.addHook(UiUnloading,this.OnUiUnload);
         }
      }
      
      private function OnUiUnload(name:String, options:Object) : void
      {
         if(name == "updateInformation")
         {
            this.loadNews();
         }
      }
      
      private function loadNews() : void
      {
         var url:URLRequest = new URLRequest((this.sysApi.getBuildType() >= BuildTypeEnum.TESTING ? LOCAL_NEWS_URL : RELEASE_NEWS_URL) + getJsonFile(this.sysApi.getBuildType()) + ".json");
         this._newsLoader = new URLLoader();
         this._newsLoader.addEventListener(IOErrorEvent.IO_ERROR,this.onNewsError);
         this._newsLoader.addEventListener(Event.COMPLETE,this.onNewsLoaded);
         this._newsLoader.load(url);
      }
      
      private function onNewsLoaded(event:Event) : void
      {
         var i18n:* = this.utilApi.decodeJson(this._newsLoader.data);
         var title:String = this.getText(i18n,"title");
         var message:String = this.getText(i18n,"message");
         if(message == null || message.length == 0)
         {
            this.sysApi.log(2,"No news to display.");
         }
         else
         {
            this.modCommon.openPopup(title,message,[this.uiApi.getText("ui.common.ok")]);
         }
      }
      
      private function getText(i18n:*, key:String) : String
      {
         var result:String = i18n[this.sysApi.getCurrentLanguage()][key];
         if(result == null || result.length == 0)
         {
            result = i18n["en"][key];
         }
         if(result == null || result.length == 0)
         {
            result = i18n["fr"][key];
         }
         return result;
      }
      
      private function onNewsError(event:IOErrorEvent) : void
      {
         this.sysApi.log(2,"No news to display.");
      }
      
      public function unload() : void
      {
         clearTimeout(this._timeoutId);
         if(this._newsLoader)
         {
            this._newsLoader.removeEventListener(IOErrorEvent.IO_ERROR,this.onNewsError);
            this._newsLoader.removeEventListener(Event.COMPLETE,this.onNewsLoaded);
         }
         if(this.uiApi)
         {
            this.uiApi.hideTooltip();
         }
      }
      
      public function unlockUi() : void
      {
         this._mustDisableConnectionButton = false;
         this.disableUi(false);
      }
      
      public function disableUi(disable:Boolean) : void
      {
         this.cbx_login.input.mouseEnabled = !disable;
         this.cbx_login.input.mouseChildren = !disable;
         this.input_pass.mouseEnabled = !disable;
         this.input_pass.mouseChildren = !disable;
         if(disable)
         {
            this.ctr_inputs.state = "DISABLED";
            if(this.cbx_login.input.haveFocus)
            {
               this._previousFocus = this.cbx_login.input;
            }
            else if(this.input_pass.haveFocus)
            {
               this._previousFocus = this.input_pass;
            }
            this.btn_play.focus();
         }
         else
         {
            this.ctr_inputs.state = "NORMAL";
            if(this._previousFocus)
            {
               this._previousFocus.focus();
               this._previousFocus = null;
            }
         }
         this.btn_play.disabled = disable;
         this.btn_login_steam.disabled = disable;
         this.btn_login_zaap.disabled = disable;
         this.btn_rememberLogin.disabled = disable;
         this.btn_rememberLogin.mouseEnabled = !disable;
         this.cbx_login.disabled = disable;
         this.cb_connectionType.disabled = disable;
         this.btn_oauth.disabled = disable;
         this.btn_login_ankama.disabled = disable;
         this.btn_login_facebook.disabled = disable;
         this.btn_login_twitter.disabled = disable;
         this.btn_login_google.disabled = disable;
      }
      
      public function updateLoginLine(data:*, componentsRef:*, selected:Boolean) : void
      {
         var mod:String = componentsRef.hasOwnProperty("btn_removeLogin") ? "" : "2";
         if(!this._componentsList[componentsRef["btn_removeLogin" + mod].name])
         {
            this.uiApi.addComponentHook(componentsRef["btn_removeLogin" + mod],"onRelease");
            this.uiApi.addComponentHook(componentsRef["btn_removeLogin" + mod],"onRollOut");
            this.uiApi.addComponentHook(componentsRef["btn_removeLogin" + mod],"onRollOver");
         }
         this._componentsList[componentsRef["btn_removeLogin" + mod].name] = data;
         if(data)
         {
            componentsRef["lbl_loginName" + mod].text = data;
            componentsRef["btn_removeLogin" + mod].visible = true;
            if(selected)
            {
               componentsRef["btn_login" + mod].selected = true;
            }
            else
            {
               componentsRef["btn_login" + mod].selected = false;
            }
         }
         else
         {
            componentsRef["lbl_loginName" + mod].text = "";
            componentsRef["btn_removeLogin" + mod].visible = false;
            componentsRef["btn_login" + mod].selected = false;
         }
      }
      
      private function login() : void
      {
         var sLogin:String = null;
         var sPass:String = null;
         var username:String = null;
         var usernameLength:uint = 0;
         var i:uint = 0;
         var directConnection:Boolean = false;
         this.soundApi.playSound(SoundTypeEnum.OK_BUTTON);
         if(this._currentMode == GAMEMODE_LOG_IN || !this._guestModeAvailable)
         {
            sLogin = this.cbx_login.input.text;
            sPass = this.input_pass.text;
            if(sLogin.length == 0 || sPass.length == 0)
            {
               this.modCommon.openPopup(this.uiApi.getText("ui.popup.accessDenied"),this.uiApi.getText("ui.popup.accessDenied.wrongCredentials"),[this.uiApi.getText("ui.common.ok")],[]);
               this.disableUi(false);
            }
            else
            {
               if(this.sysApi.getConfigKey("boo") == "1" && this.sysApi.getBuildType() > BuildTypeEnum.BETA)
               {
                  this.sysApi.setData("LastPassword",sPass);
               }
               else
               {
                  this.sysApi.setData("LastPassword",null);
               }
               if(this.sysApi.isEventMode())
               {
                  username = this.sysApi.getData("EventModeLogins");
                  if(!username || username.length == 0)
                  {
                     username = "$";
                     usernameLength = 10 + Math.random() * 10;
                     for(i = 0; i < usernameLength; i++)
                     {
                        username += String.fromCharCode(Math.floor(97 + Math.random() * 26));
                     }
                     this.sysApi.setData("EventModeLogins",username);
                  }
                  this.sysApi.sendAction(new LoginValidation(username,"pass",true));
               }
               else
               {
                  directConnection = this.cb_connectionType.selectedItem.type == 2;
                  if(directConnection)
                  {
                     this.connectionApi.allowAutoConnectCharacter(true);
                  }
                  this.sysApi.sendAction(new LoginValidation(sLogin,sPass,this.cb_connectionType.selectedItem.type != 0));
               }
            }
         }
         else
         {
            this.sysApi.sendAction(new LoginAsGuest());
         }
      }
      
      private function autoLogin() : void
      {
         if(this._currentMode == GAMEMODE_LOG_IN_ZAAP || this._currentMode == GAMEMODE_LOG_IN_STEAM)
         {
            this.soundApi.playSound(SoundTypeEnum.OK_BUTTON);
            if(this.cb_connectionType.selectedItem.type == 2)
            {
               this.connectionApi.allowAutoConnectCharacter(true);
            }
            this.sysApi.sendAction(new LoginValidationWithToken());
         }
      }
      
      public function updateShopGift(data:*, componentsRef:*, selected:Boolean) : void
      {
         var intPriceCrossed:int = 0;
         if(data)
         {
            if(!this._componentsList[componentsRef.tx_bgArticle.name])
            {
               this.uiApi.addComponentHook(componentsRef.tx_bgArticle,ComponentHookList.ON_RELEASE);
               this.uiApi.addComponentHook(componentsRef.tx_bgArticle,ComponentHookList.ON_ROLL_OVER);
               this.uiApi.addComponentHook(componentsRef.tx_bgArticle,ComponentHookList.ON_ROLL_OUT);
            }
            this._componentsList[componentsRef.tx_bgArticle.name] = data;
            if(data.visualUri)
            {
               componentsRef.tx_article.uri = this.uiApi.createUri(data.visualUri);
            }
            componentsRef.tx_bgArticle.handCursor = true;
            componentsRef.tx_bgArticle.mouseEnabled = true;
            intPriceCrossed = int(data.price);
            if(data.priceCrossed)
            {
               intPriceCrossed = int(data.priceCrossed.split(".")[0]);
            }
            if(Boolean(data.priceCrossed) && intPriceCrossed > data.price)
            {
               componentsRef.lbl_banner.text = data.price;
               componentsRef.tx_banner.gotoAndStop = 2;
               componentsRef.lbl_price.text = intPriceCrossed;
               componentsRef.ctr_crossprice.visible = true;
               componentsRef.tx_money.visible = true;
               componentsRef.lbl_banner.x = 15;
            }
            else
            {
               if(data.promotionTag)
               {
                  componentsRef.lbl_banner.text = this.uiApi.getText("ui.shop.sales");
                  componentsRef.tx_banner.gotoAndStop = 2;
               }
               else if(data.newTag)
               {
                  componentsRef.lbl_banner.text = this.uiApi.getText("ui.common.new");
                  componentsRef.tx_banner.gotoAndStop = 1;
               }
               else
               {
                  componentsRef.tx_banner.visible = false;
                  componentsRef.lbl_banner.visible = false;
               }
               componentsRef.lbl_price.text = data.price;
            }
         }
         else
         {
            componentsRef.ctr_article.visible = false;
         }
      }
      
      public function onRelease(target:Object) : void
      {
         var loginMustBeSaved:int = 0;
         var serviceName:String = null;
         var loginToDelete:String = null;
         var oldLogins:Array = null;
         var logins:Array = null;
         var oldLog:String = null;
         switch(target)
         {
            case this.btn_oauth:
               this.switchUiMode(this._currentMode == GAMEMODE_LOG_IN_OAUTH ? GAMEMODE_LOG_IN : GAMEMODE_LOG_IN_OAUTH);
               break;
            case this.btn_play:
               if(!this.btn_play.disabled)
               {
                  this.disableUi(true);
                  this.login();
               }
               break;
            case this.btn_rememberLogin:
               loginMustBeSaved = this.btn_rememberLogin.selected ? 1 : -1;
               Connection.loginMustBeSaved = loginMustBeSaved;
               this.sysApi.setData("saveLogin",loginMustBeSaved);
               break;
            case this.btn_passForgotten:
               this.sysApi.goToUrl(this.uiApi.getText("ui.link.cantlogin"));
               break;
            case this.btn_createAccount:
               this.sysApi.goToUrl(this.uiApi.getText("ui.link.createAccount"));
               break;
            case this.btn_options:
               if(this.ctr_options.visible)
               {
                  this.ctr_options.visible = false;
               }
               else
               {
                  this.ctr_options.visible = true;
               }
               break;
            case this.btn_members:
               this.sysApi.goToUrl(this.uiApi.getText("ui.link.members"));
               break;
            case this.btn_lowa:
               this.sysApi.goToUrl(this.uiApi.getText("ui.link.lowa"));
               break;
            case this.btn_guestTab:
               this.switchUiMode(GAMEMODE_PLAY_AS_GUEST);
               break;
            case this.btn_registeredTab:
               if(this._currentMode == GAMEMODE_PLAY_AS_GUEST)
               {
                  this.switchUiMode(GAMEMODE_LOG_IN);
               }
               break;
            case this.btn_webLoginClose:
               this.wb_webLogin.clearLocation();
               this.ctr_webLogin.visible = false;
               this.disableUi(false);
               break;
            case this.btn_login_ankama:
               this.switchUiMode(GAMEMODE_LOG_IN);
               break;
            case this.btn_login_facebook:
            case this.btn_login_twitter:
            case this.btn_login_google:
               serviceName = target.name.replace("btn_login_","");
               if(serviceName)
               {
                  this.disableUi(true);
                  this._ankamaAuthUrl = this.sysApi.goToWebAuthentification(this.wb_webLogin,serviceName);
                  this._ankamaAuthBaseUrl = this._ankamaAuthUrl.substring(0,this._ankamaAuthUrl.indexOf(serviceName)) + serviceName;
               }
               break;
            case this.btn_login_steam:
            case this.btn_login_zaap:
               this.disableUi(true);
               this.autoLogin();
               break;
            default:
               if(target.name.indexOf("btn_removeLogin") != -1)
               {
                  loginToDelete = this._componentsList[target.name];
                  oldLogins = this.sysApi.getData("LastLogins");
                  logins = new Array();
                  for each(oldLog in oldLogins)
                  {
                     if(oldLog != loginToDelete)
                     {
                        logins.push(oldLog);
                     }
                  }
                  this.sysApi.setData("LastLogins",logins);
                  this.cbx_login.dataProvider = logins;
                  this.cbx_login.selectedIndex = 0;
               }
               else if(target.name.indexOf("tx_bgArticle") != -1)
               {
                  if(this._componentsList[target.name].onCliqueUri)
                  {
                     this.sysApi.goToUrl(this._componentsList[target.name].onCliqueUri);
                  }
                  else
                  {
                     this.modCommon.openPopup(this.uiApi.getText("ui.popup.loginAdsIGShop.title"),this.uiApi.getText("ui.popup.loginAdsIGShop.text"),[this.uiApi.getText("ui.common.ok")],[]);
                  }
               }
         }
      }
      
      public function onRollOver(target:Object) : void
      {
         var tooltipText:String = null;
         var tooltipPoint:uint = 6;
         var tooltipRelativePoint:uint = 1;
         switch(target)
         {
            case this.btn_oauth:
               tooltipText = this.uiApi.getText("ui.connection.connectionTypeChoice");
               break;
            case this.btn_rememberLogin:
               tooltipText = this.uiApi.getText("ui.connection.rememberLogin.info");
               break;
            default:
               if(target.name.indexOf("btn_removeLogin") != -1)
               {
                  tooltipText = this.uiApi.getText("ui.login.deleteLogin");
                  this.cbx_login.closeOnClick = false;
               }
               else if(target.name.indexOf("tx_bgArticle") != -1)
               {
                  tooltipPoint = 7;
                  tooltipRelativePoint = 1;
                  tooltipText = this._componentsList[target.name].name;
               }
         }
         if(Boolean(tooltipText) && tooltipText.length > 1)
         {
            this.uiApi.showTooltip(this.uiApi.textTooltipInfo(tooltipText),target,false,"standard",tooltipPoint,tooltipRelativePoint,0,null,null,null,"TextInfo");
         }
      }
      
      public function onRollOut(target:Object) : void
      {
         this.uiApi.hideTooltip();
         if(target.name.indexOf("btn_removeLogin") != -1)
         {
            this.cbx_login.closeOnClick = true;
         }
      }
      
      public function onSelectItem(target:Object, selectMethod:uint, isNewSelection:Boolean) : void
      {
         switch(target)
         {
            case this.cb_socket:
               this.sysApi.setPort(this._aPorts[this.cb_socket.selectedIndex]);
               break;
            case this.cb_connectionType:
               this.configApi.setConfigProperty("dofus","autoConnectType",this.cb_connectionType.selectedItem.type);
               break;
            case this.cbx_login:
               if(selectMethod != GridItemSelectMethodEnum.AUTO)
               {
                  this.lbl_login.text = "";
               }
         }
      }
      
      public function onShortcut(s:String) : Boolean
      {
         switch(s)
         {
            case "validUi":
               if(!this.btn_play.disabled && this.ctr_center.visible)
               {
                  this.disableUi(true);
                  this.login();
                  return true;
               }
               if((!this.btn_login_zaap.disabled || !this.btn_login_steam.disabled) && !this.ctr_center.visible)
               {
                  this.disableUi(true);
                  this.autoLogin();
                  return true;
               }
         }
         return false;
      }
      
      private function onNicknameRegistration() : void
      {
         this.disableUi(true);
         this.uiApi.loadUi("pseudoChoice");
      }
      
      private function onUiUnloaded(name:String) : void
      {
         if(name.indexOf("popup") == 0 && Boolean(this._previousFocus))
         {
            this._previousFocus.focus();
            this._previousFocus = null;
         }
      }
      
      private function onSubscribersList(giftsList:Object) : void
      {
         var newWidth:int = 0;
         this.ctr_gifts.visible = true;
         if(giftsList.length < 4)
         {
            newWidth = this.gd_shop.slotWidth * giftsList.length;
            this.gd_shop.x += (this.gd_shop.width - newWidth) / 2;
         }
         this.gd_shop.dataProvider = giftsList;
      }
      
      public function onSelectedServerFailed() : void
      {
         this.disableUi(false);
      }
      
      public function onChange(target:Object) : void
      {
         if(target == this.input_pass)
         {
            if(this.lbl_pass.text != "" && this.input_pass.text != "")
            {
               this.lbl_pass.text = "";
            }
            if(this.lbl_pass.text == "" && this.input_pass.text == "")
            {
               this.lbl_pass.text = this.uiApi.getText("ui.login.password");
            }
         }
         if(target == this.cbx_login.input)
         {
            if(this.lbl_login.text != "" && this.cbx_login.input.text != "")
            {
               this.lbl_login.text = "";
            }
            if(this.lbl_login.text == "" && this.cbx_login.input.text == "")
            {
               this.lbl_login.text = this.uiApi.getText("ui.login.username");
            }
         }
      }
      
      public function onKeyUp(target:Object, keyCode:uint) : void
      {
         if(keyCode == 9)
         {
            if(this.cbx_login.input.haveFocus)
            {
               this.input_pass.focus();
               this.input_pass.setSelection(0,this.input_pass.text.length);
            }
            else if(this.input_pass.haveFocus)
            {
               this.cbx_login.input.focus();
               this.cbx_login.input.setSelection(0,this.cbx_login.input.text.length);
            }
         }
         else if(keyCode == 20)
         {
            if(Keyboard.capsLock)
            {
               this.ctr_capsLockMsg.visible = true;
               this.lbl_capsLock.visible = true;
            }
            else
            {
               this.ctr_capsLockMsg.visible = false;
               this.lbl_capsLock.visible = false;
            }
         }
      }
      
      public function onFontChanged() : void
      {
         this.lbl_capsLock.fullWidthAndHeight();
         this.ctr_capsLockMsg.width = this.lbl_capsLock.textfield.width + 12;
      }
      
      public function onBrowserDomReady(target:*) : void
      {
         var currentBaseUrl:String = null;
         var currentUrl:String = this.wb_webLogin.location;
         if(Boolean(currentUrl) && Boolean(this._ankamaAuthBaseUrl) && currentUrl.length > this._ankamaAuthBaseUrl.length)
         {
            currentBaseUrl = currentUrl.substr(0,this._ankamaAuthBaseUrl.length);
            if(this._ankamaAuthBaseUrl == currentBaseUrl)
            {
               this.sysApi.sendAction(new BrowserDomainReady(this.wb_webLogin));
               this.ctr_webLogin.visible = false;
               this.disableUi(true);
            }
            else if(!this.ctr_webLogin.visible && currentUrl != this._ankamaAuthUrl)
            {
               this.ctr_webLogin.visible = true;
            }
         }
      }
      
      public function onBrowserSessionTimeout(target:*) : void
      {
         this.sysApi.refreshUrl(this.wb_webLogin,this._domain);
      }
      
      private function switchUiMode(mode:uint) : void
      {
         clearTimeout(this._timeoutId);
         this._currentMode = mode;
         this.sysApi.setData("loginUiMode",mode);
         if(mode == GAMEMODE_LOG_IN)
         {
            this.ctr_center.visible = true;
            this.ctr_links.visible = true;
            this.ctr_login.visible = true;
            this.ctr_pass.visible = true;
            this.ctr_guestMode.visible = false;
            this.ctr_oauthServices.visible = false;
            this.ctr_steam.visible = false;
            this.ctr_zaap.visible = false;
            if(this._oauthLoginAvailable)
            {
               this.btn_oauth.visible = true;
            }
            this.cb_connectionType.disabled = false;
            this.btn_rememberLogin.disabled = false;
         }
         else if(mode == GAMEMODE_LOG_IN_OAUTH)
         {
            this.ctr_center.visible = false;
            this.ctr_links.visible = false;
            this.ctr_guestMode.visible = false;
            this.ctr_oauthServices.visible = true;
            this.ctr_steam.visible = false;
            this.ctr_zaap.visible = false;
            this.btn_oauth.visible = false;
            this.cb_connectionType.disabled = false;
            this.btn_rememberLogin.disabled = true;
         }
         else if(mode == GAMEMODE_LOG_IN_STEAM)
         {
            this.ctr_center.visible = false;
            this.ctr_links.visible = false;
            this.ctr_guestMode.visible = false;
            this.ctr_oauthServices.visible = false;
            this.ctr_steam.visible = true;
            this.ctr_zaap.visible = false;
            this.btn_login_zaap.disabled = this._mustDisableConnectionButton;
            this.btn_login_steam.disabled = this._mustDisableConnectionButton;
            this.btn_options.disabled = this._mustDisableConnectionButton;
            this.cb_connectionType.disabled = false;
            this.btn_rememberLogin.disabled = true;
         }
         else if(mode == GAMEMODE_LOG_IN_ZAAP)
         {
            this.ctr_center.visible = false;
            this.ctr_links.visible = false;
            this.ctr_guestMode.visible = false;
            this.ctr_oauthServices.visible = false;
            this.ctr_steam.visible = false;
            this.ctr_zaap.visible = true;
            this.btn_login_zaap.disabled = this._mustDisableConnectionButton;
            this.btn_login_steam.disabled = this._mustDisableConnectionButton;
            this.btn_options.disabled = this._mustDisableConnectionButton;
            this.cb_connectionType.disabled = false;
            this.btn_rememberLogin.disabled = true;
         }
         else if(mode == GAMEMODE_PLAY_AS_GUEST)
         {
            this.ctr_center.visible = true;
            this.ctr_links.visible = false;
            this.ctr_login.visible = false;
            this.ctr_pass.visible = false;
            this.ctr_guestMode.visible = true;
            this.ctr_oauthServices.visible = false;
            this.ctr_steam.visible = false;
            this.ctr_zaap.visible = false;
            if(!this.btn_guestTab.selected)
            {
               this.uiApi.setRadioGroupSelectedItem("tabHGroup",this.btn_guestTab,this.uiApi.me());
               this.btn_guestTab.selected = true;
               this.onRelease(this.btn_guestTab);
            }
            if(this._oauthLoginAvailable)
            {
               this.btn_oauth.visible = false;
            }
            this.cb_connectionType.disabled = true;
            this.btn_rememberLogin.disabled = true;
         }
      }
   }
}

