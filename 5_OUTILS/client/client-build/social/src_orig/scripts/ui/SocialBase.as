package ui
{
   import com.ankamagames.berilia.api.UiApi;
   import com.ankamagames.berilia.components.Label;
   import com.ankamagames.berilia.types.graphic.ButtonContainer;
   import com.ankamagames.berilia.types.graphic.GraphicContainer;
   import com.ankamagames.berilia.types.graphic.UiRootContainer;
   import com.ankamagames.dofus.datacenter.misc.OptionalFeature;
   import com.ankamagames.dofus.uiApi.BindsApi;
   import com.ankamagames.dofus.uiApi.ConfigApi;
   import com.ankamagames.dofus.uiApi.DataApi;
   import com.ankamagames.dofus.uiApi.PlayedCharacterApi;
   import com.ankamagames.dofus.uiApi.SocialApi;
   import com.ankamagames.dofus.uiApi.SoundApi;
   import com.ankamagames.dofus.uiApi.SystemApi;
   import com.ankamagames.dofusModuleLibrary.enum.SoundEnum;
   import com.ankamagames.dofusModuleLibrary.enum.SoundTypeEnum;
   import d2actions.*;
   import d2enums.DataEnum;
   import d2enums.LocationEnum;
   import d2hooks.*;
   
   public class SocialBase
   {
      
      private static var _shortcutColor:String;
      
      public var bindsApi:BindsApi;
      
      public var sysApi:SystemApi;
      
      public var uiApi:UiApi;
      
      public var socialApi:SocialApi;
      
      public var soundApi:SoundApi;
      
      public var playerApi:PlayedCharacterApi;
      
      public var configApi:ConfigApi;
      
      public var dataApi:DataApi;
      
      private var _currentTabUi:int = -1;
      
      private var _currentSubTabUi:int = -1;
      
      private var _hasGuild:Boolean;
      
      private var _hasAlliance:Boolean;
      
      private var _hasSpouse:Boolean;
      
      private var _friendList:Object;
      
      public var uiCtr:GraphicContainer;
      
      public var btn_close:ButtonContainer;
      
      public var btn_help:ButtonContainer;
      
      public var btn_tabFriend:ButtonContainer;
      
      public var btn_tabGuild:ButtonContainer;
      
      public var btn_tabAlliance:ButtonContainer;
      
      public var btn_tabDirectory:ButtonContainer;
      
      public var btn_tabDare:ButtonContainer;
      
      public var lbl_btn_tabFriend:Label;
      
      public var lbl_btn_tabGuild:Label;
      
      public var lbl_btn_tabAlliance:Label;
      
      public var lbl_btn_tabDirectory:Label;
      
      public var lbl_btn_tabDare:Label;
      
      public function SocialBase()
      {
         super();
      }
      
      public function main(params:Object) : void
      {
         var shortcut:String = null;
         var restoreSnapshotAfterLoading:Boolean = false;
         this.btn_close.soundId = SoundEnum.CANCEL_BUTTON;
         this.soundApi.playSound(SoundTypeEnum.SOCIAL_OPEN);
         this.btn_tabFriend.soundId = SoundEnum.TAB;
         this.btn_tabGuild.soundId = SoundEnum.TAB;
         this.btn_tabAlliance.soundId = SoundEnum.TAB;
         this.btn_tabDirectory.soundId = SoundEnum.TAB;
         this.btn_tabDare.soundId = SoundEnum.TAB;
         this.sysApi.addHook(SpouseUpdated,this.onSpouseUpdated);
         this.sysApi.addHook(GuildMembershipUpdated,this.onGuildMembershipUpdated);
         this.sysApi.addHook(AllianceMembershipUpdated,this.onAllianceMembershipUpdated);
         this.uiApi.addComponentHook(this.btn_tabFriend,"onRelease");
         this.uiApi.addComponentHook(this.btn_tabFriend,"onRollOver");
         this.uiApi.addComponentHook(this.btn_tabFriend,"onRollOut");
         this.uiApi.addComponentHook(this.btn_tabGuild,"onRelease");
         this.uiApi.addComponentHook(this.btn_tabGuild,"onRollOver");
         this.uiApi.addComponentHook(this.btn_tabGuild,"onRollOut");
         this.uiApi.addComponentHook(this.btn_tabAlliance,"onRelease");
         this.uiApi.addComponentHook(this.btn_tabAlliance,"onRollOver");
         this.uiApi.addComponentHook(this.btn_tabAlliance,"onRollOut");
         this.uiApi.addComponentHook(this.btn_tabDirectory,"onRelease");
         this.uiApi.addComponentHook(this.btn_tabDirectory,"onRollOver");
         this.uiApi.addComponentHook(this.btn_tabDirectory,"onRollOut");
         this.uiApi.addComponentHook(this.btn_tabDare,"onRelease");
         this.uiApi.addComponentHook(this.btn_tabDare,"onRollOver");
         this.uiApi.addComponentHook(this.btn_tabDare,"onRollOut");
         this.uiApi.addComponentHook(this.btn_close,"onRelease");
         this.uiApi.addComponentHook(this.btn_help,"onRelease");
         this.uiApi.addShortcutHook("closeUi",this.onShortcut);
         this.uiApi.addShortcutHook("openSocialFriends",this.onShortcut);
         this.uiApi.addShortcutHook("openSocialGuild",this.onShortcut);
         this.uiApi.addShortcutHook("openSocialAlliance",this.onShortcut);
         var feature:OptionalFeature = this.dataApi.getOptionalFeatureByKeyword("social.dares");
         if(Boolean(feature) && this.configApi.isOptionalFeatureActive(feature.id))
         {
            this.btn_tabDare.visible = true;
         }
         else
         {
            this.btn_tabDare.visible = false;
         }
         this._hasGuild = this.socialApi.hasGuild();
         this._hasAlliance = this.socialApi.hasAlliance();
         this._hasSpouse = this.socialApi.hasSpouse();
         this.displayTabs();
         this._currentTabUi = -2;
         if(Boolean(params) && params.hasOwnProperty("tab"))
         {
            if(params.hasOwnProperty("subTab"))
            {
               restoreSnapshotAfterLoading = this.uiApi.me().restoreSnapshotAfterLoading;
               if(params.hasOwnProperty("restoreSnapshotAfterLoading"))
               {
                  restoreSnapshotAfterLoading = Boolean(params.restoreSnapshotAfterLoading);
               }
               if(params.hasOwnProperty("params") && params.params != null)
               {
                  this.openTab(params.tab,params.subTab,params.params,restoreSnapshotAfterLoading);
               }
               else
               {
                  this.openTab(params.tab,params.subTab,null,restoreSnapshotAfterLoading);
               }
            }
            else
            {
               this.openTab(params.tab,0,null,false);
            }
         }
         else
         {
            this.openTab(-1);
         }
         if(!_shortcutColor)
         {
            _shortcutColor = this.sysApi.getConfigEntry("colors.shortcut");
            _shortcutColor = _shortcutColor.replace("0x","#");
         }
         this.lbl_btn_tabFriend.text = this.uiApi.getText("ui.common.friends");
         shortcut = this.bindsApi.getShortcutBindStr("openSocialFriends");
         if(shortcut != "")
         {
            this.lbl_btn_tabFriend.text += " <font color=\'" + _shortcutColor + "\'>(" + shortcut + ")</font>";
         }
         this.lbl_btn_tabGuild.text = this.uiApi.getText("ui.common.guild");
         shortcut = this.bindsApi.getShortcutBindStr("openSocialGuild");
         if(shortcut != "")
         {
            this.lbl_btn_tabGuild.text += " <font color=\'" + _shortcutColor + "\'>(" + shortcut + ")</font>";
         }
         this.lbl_btn_tabAlliance.text = this.uiApi.getText("ui.common.alliance");
         shortcut = this.bindsApi.getShortcutBindStr("openSocialAlliance");
         if(shortcut != "")
         {
            this.lbl_btn_tabAlliance.text += " <font color=\'" + _shortcutColor + "\'>(" + shortcut + ")</font>";
         }
         this.lbl_btn_tabDirectory.text = this.uiApi.getText("ui.common.directory");
         this.lbl_btn_tabDare.text = this.uiApi.getText("ui.dare.dare");
      }
      
      public function unload() : void
      {
         this.sysApi.sendAction(new GuildFightLeaveRequest(0,this.playerApi.id(),true));
         this.sysApi.sendAction(new PrismFightJoinLeaveRequest(0,false));
         this.soundApi.playSound(SoundTypeEnum.CLOSE_WINDOW);
         this.closeTab(this._currentTabUi);
      }
      
      public function getTab() : int
      {
         return this._currentTabUi;
      }
      
      public function getSubTab() : int
      {
         return this._currentSubTabUi;
      }
      
      public function setSubTab(subTab:int) : void
      {
         this._currentSubTabUi = subTab;
      }
      
      private function displayTabs() : void
      {
         if(this.btn_tabGuild.softDisabled == this._hasGuild)
         {
            this.btn_tabGuild.softDisabled = !this._hasGuild;
         }
         if(this.btn_tabAlliance.softDisabled == this._hasAlliance)
         {
            this.btn_tabAlliance.softDisabled = !this._hasAlliance;
         }
         if(this._currentTabUi == 1 && this.btn_tabGuild.softDisabled)
         {
            this.openTab(0);
         }
         if(this._currentTabUi == 2 && this.btn_tabAlliance.softDisabled)
         {
            this.openTab(0);
         }
      }
      
      public function openTab(tab:int = -1, subTab:int = 0, params:Object = null, restoreSnapshot:Boolean = true) : void
      {
         var lastTab:uint = 0;
         if(tab != -1 && (Boolean(this._currentTabUi == tab && this._currentSubTabUi == subTab) || Boolean(this.getButtonByTab(tab).disabled) || Boolean(this.getButtonByTab(tab).softDisabled)))
         {
            return;
         }
         if(this._currentTabUi > -1)
         {
            this.uiApi.unloadUi("subSocialUi");
         }
         if(tab == -1)
         {
            lastTab = this.sysApi.getData("lastSocialTab");
            if(Boolean(lastTab) && !this.getButtonByTab(lastTab).disabled)
            {
               this._currentTabUi = lastTab;
            }
            else
            {
               this._currentTabUi = DataEnum.SOCIAL_TAB_FRIENDS_ID;
            }
            if(Boolean(lastTab) && (lastTab == DataEnum.SOCIAL_TAB_GUILD_ID && !this.socialApi.hasGuild() || lastTab == DataEnum.SOCIAL_TAB_ALLIANCE_ID && !this.socialApi.hasAlliance()))
            {
               this._currentTabUi = DataEnum.SOCIAL_TAB_FRIENDS_ID;
            }
         }
         else
         {
            this._currentTabUi = tab;
         }
         if(this._currentTabUi == DataEnum.SOCIAL_TAB_FRIENDS_ID)
         {
            this.sysApi.sendAction(new FriendsListRequest());
            this.sysApi.sendAction(new ContactsListRequest());
            this.sysApi.sendAction(new EnemiesListRequest());
         }
         else if(this._currentTabUi == DataEnum.SOCIAL_TAB_ALLIANCE_ID)
         {
            this.sysApi.sendAction(new AllianceInsiderInfoRequest());
         }
         else if(this._currentTabUi == DataEnum.SOCIAL_TAB_SPOUSE_ID)
         {
            this.sysApi.sendAction(new SpouseRequest());
         }
         else if(this._currentTabUi != DataEnum.SOCIAL_TAB_DIRECTORY_ID)
         {
            if(this._currentTabUi == DataEnum.SOCIAL_TAB_DARE_ID)
            {
               this.sysApi.log(2,"Récupération de la liste complete des défis ?");
            }
         }
         this.sysApi.setData("lastSocialTab",this._currentTabUi);
         this.uiCtr.getUi().restoreSnapshotAfterLoading = restoreSnapshot;
         this.uiApi.loadUiInside(this.getUiNameByTab(this._currentTabUi),this.uiCtr,"subSocialUi",[subTab,params]) as UiRootContainer;
         this.uiApi.setRadioGroupSelectedItem("tabHGroup",this.getButtonByTab(this._currentTabUi),this.uiApi.me());
         this.getButtonByTab(this._currentTabUi).selected = true;
      }
      
      private function closeTab(tab:uint) : void
      {
         this.uiApi.unloadUi("subSocialUi");
      }
      
      private function getButtonByTab(tab:uint) : Object
      {
         var returnButton:Object = null;
         switch(tab)
         {
            case DataEnum.SOCIAL_TAB_FRIENDS_ID:
               returnButton = this.btn_tabFriend;
               break;
            case DataEnum.SOCIAL_TAB_GUILD_ID:
               returnButton = this.btn_tabGuild;
               break;
            case DataEnum.SOCIAL_TAB_ALLIANCE_ID:
               returnButton = this.btn_tabAlliance;
               break;
            case DataEnum.SOCIAL_TAB_DIRECTORY_ID:
               returnButton = this.btn_tabDirectory;
               break;
            case DataEnum.SOCIAL_TAB_DARE_ID:
               returnButton = this.btn_tabDare;
         }
         return returnButton;
      }
      
      private function getUiNameByTab(tab:uint) : String
      {
         var uiName:String = null;
         switch(tab)
         {
            case DataEnum.SOCIAL_TAB_FRIENDS_ID:
               uiName = "friends";
               break;
            case DataEnum.SOCIAL_TAB_GUILD_ID:
               uiName = "guild";
               break;
            case DataEnum.SOCIAL_TAB_ALLIANCE_ID:
               uiName = "alliance";
               break;
            case DataEnum.SOCIAL_TAB_SPOUSE_ID:
               uiName = "spouse";
               break;
            case DataEnum.SOCIAL_TAB_DIRECTORY_ID:
               uiName = "directory";
               break;
            case DataEnum.SOCIAL_TAB_DARE_ID:
               uiName = "dare";
         }
         return uiName;
      }
      
      private function closeSocial() : void
      {
         this.uiApi.unloadUi(this.uiApi.me().name);
      }
      
      private function onGuildMembershipUpdated(hasGuild:Boolean) : void
      {
         this._hasGuild = hasGuild;
         this.displayTabs();
      }
      
      private function onAllianceMembershipUpdated(hasAlliance:Boolean) : void
      {
         this._hasAlliance = hasAlliance;
         this.displayTabs();
      }
      
      private function onSpouseUpdated() : void
      {
         this._hasSpouse = this.socialApi.hasSpouse();
         this.displayTabs();
      }
      
      public function onRelease(target:Object) : void
      {
         switch(target)
         {
            case this.btn_close:
               this.closeSocial();
               break;
            case this.btn_help:
               this.uiApi.me().childUiRoot.uiClass.selectWhichTabHintsToDisplay();
               break;
            case this.btn_tabFriend:
               this.openTab(DataEnum.SOCIAL_TAB_FRIENDS_ID);
               break;
            case this.btn_tabGuild:
               this.openTab(DataEnum.SOCIAL_TAB_GUILD_ID);
               break;
            case this.btn_tabAlliance:
               this.openTab(DataEnum.SOCIAL_TAB_ALLIANCE_ID);
               break;
            case this.btn_tabDirectory:
               this.openTab(DataEnum.SOCIAL_TAB_DIRECTORY_ID);
               break;
            case this.btn_tabDare:
               this.openTab(DataEnum.SOCIAL_TAB_DARE_ID);
         }
      }
      
      public function onShortcut(s:String) : Boolean
      {
         switch(s)
         {
            case "openSocialFriends":
               if(this._currentTabUi == DataEnum.SOCIAL_TAB_FRIENDS_ID)
               {
                  this.uiApi.unloadUi(this.uiApi.me().name);
               }
               else
               {
                  this.openTab(DataEnum.SOCIAL_TAB_FRIENDS_ID);
               }
               return true;
            case "openSocialGuild":
               if(this._currentTabUi == DataEnum.SOCIAL_TAB_GUILD_ID)
               {
                  this.uiApi.unloadUi(this.uiApi.me().name);
               }
               else
               {
                  this.openTab(DataEnum.SOCIAL_TAB_GUILD_ID);
               }
               return true;
            case "openSocialAlliance":
               if(this._currentTabUi == DataEnum.SOCIAL_TAB_ALLIANCE_ID)
               {
                  this.uiApi.unloadUi(this.uiApi.me().name);
               }
               else
               {
                  this.openTab(DataEnum.SOCIAL_TAB_ALLIANCE_ID);
               }
               return true;
            case "closeUi":
               this.closeSocial();
               return true;
            default:
               return false;
         }
      }
      
      public function onRollOver(target:Object) : void
      {
         var point:uint = LocationEnum.POINT_LEFT;
         var relPoint:uint = LocationEnum.POINT_RIGHT;
         var description:String = "";
         var tooltipTxt:String = "";
         switch(target)
         {
            case this.btn_tabGuild:
               description = this.uiApi.getText("ui.banner.lockBtn.guild");
               break;
            case this.btn_tabAlliance:
               description = this.uiApi.getText("ui.banner.lockBtn.alliance");
         }
         if(target.softDisabled)
         {
            if(description)
            {
               tooltipTxt = description;
            }
         }
         if(Boolean(tooltipTxt) && tooltipTxt != "")
         {
            this.uiApi.showTooltip(this.uiApi.textTooltipInfo(tooltipTxt),target,false,"standard",point,relPoint,3,null,null,null,"TextInfo");
         }
      }
      
      public function onRollOut(target:Object) : void
      {
         this.uiApi.hideTooltip();
      }
   }
}

