package ui
{
   import com.ankamagames.berilia.api.UiApi;
   import com.ankamagames.berilia.components.Grid;
   import com.ankamagames.berilia.components.Input;
   import com.ankamagames.berilia.components.Label;
   import com.ankamagames.berilia.types.graphic.ButtonContainer;
   import com.ankamagames.dofus.datacenter.arena.ArenaLeague;
   import com.ankamagames.dofus.datacenter.communication.Smiley;
   import com.ankamagames.dofus.datacenter.guild.EmblemSymbol;
   import com.ankamagames.dofus.internalDatacenter.arena.ArenaRankInfosWrapper;
   import com.ankamagames.dofus.uiApi.ChatApi;
   import com.ankamagames.dofus.uiApi.DataApi;
   import com.ankamagames.dofus.uiApi.SocialApi;
   import com.ankamagames.dofus.uiApi.SystemApi;
   import com.ankamagames.dofus.uiApi.UiTutoApi;
   import com.ankamagames.dofus.uiApi.UtilApi;
   import com.ankamagames.dofusModuleLibrary.enum.SoundEnum;
   import d2actions.*;
   import d2enums.ComponentHookList;
   import d2enums.LocationEnum;
   import d2enums.PlayerStateEnum;
   import d2enums.PlayerStatusEnum;
   import d2enums.ProtocolConstantsEnum;
   import d2hooks.*;
   import flash.utils.Dictionary;
   
   public class Friends
   {
      
      private static const SOCIAL_LEAGUE_ICON_PREFIX:String = "social_";
      
      public var sysApi:SystemApi;
      
      public var uiApi:UiApi;
      
      public var hintsApi:UiTutoApi;
      
      [Module(name="Ankama_Common")]
      public var modCommon:Object;
      
      public var socialApi:SocialApi;
      
      public var dataApi:DataApi;
      
      public var chatApi:ChatApi;
      
      public var utilApi:UtilApi;
      
      private var _friendsList:Object;
      
      private var _contactsList:Object;
      
      private var _enemiesList:Object;
      
      private var _ignoredList:Object;
      
      private var _nCurrentTab:uint = 0;
      
      private var _bShowOfflineGuys:Boolean = false;
      
      private var _bDescendingSort:Boolean = false;
      
      private var _interactiveComponentsList:Dictionary = new Dictionary(true);
      
      private var _iconsPath:String;
      
      private var _friendIdWaitingForKick:int = -1;
      
      private var _enemyIdWaitingForKick:int = -1;
      
      private var _bgLevelUri:Object;
      
      private var _bgPrestigeUri:Object;
      
      private var _currentTabName:String;
      
      public var btn_friend:ButtonContainer;
      
      public var btn_contact:ButtonContainer;
      
      public var btn_enemy:ButtonContainer;
      
      public var btn_ignored:ButtonContainer;
      
      public var inp_add:Input;
      
      public var lbl_add:Label;
      
      public var btn_add:ButtonContainer;
      
      public var gd_list:Grid;
      
      public var btn_showOfflineFriends:ButtonContainer;
      
      public var btn_warnWhenFriendIsOnline:ButtonContainer;
      
      public var btn_shareStatus:ButtonContainer;
      
      public var btn_tabBreed:ButtonContainer;
      
      public var btn_tabName:ButtonContainer;
      
      public var btn_tabLevel:ButtonContainer;
      
      public var btn_tabGuild:ButtonContainer;
      
      public var btn_tabAchievement:ButtonContainer;
      
      public var btn_tabLeague:ButtonContainer;
      
      public var btn_tabState:ButtonContainer;
      
      public function Friends()
      {
         super();
      }
      
      public function get currentTabName() : String
      {
         return this._currentTabName;
      }
      
      public function set currentTabName(value:String) : void
      {
         this._currentTabName = value;
      }
      
      public function main(... params) : void
      {
         this.btn_add.soundId = SoundEnum.SPEC_BUTTON;
         this.btn_friend.soundId = SoundEnum.TAB;
         this.btn_contact.soundId = SoundEnum.TAB;
         this.btn_enemy.soundId = SoundEnum.TAB;
         this.btn_ignored.soundId = SoundEnum.TAB;
         this.sysApi.addHook(FriendsListUpdated,this.onFriendsUpdated);
         this.sysApi.addHook(ContactsListUpdated,this.onContactsUpdated);
         this.sysApi.addHook(EnemiesListUpdated,this.onEnemiesUpdated);
         this.sysApi.addHook(IgnoredListUpdated,this.onIgnoredUpdated);
         this.sysApi.addHook(FriendWarningState,this.onFriendWarningState);
         this.sysApi.addHook(ShareStatusState,this.onStatusShareState);
         this.sysApi.addHook(FocusChange,this.onFocusChange);
         this.uiApi.addComponentHook(this.btn_add,ComponentHookList.ON_RELEASE);
         this.uiApi.addComponentHook(this.btn_showOfflineFriends,ComponentHookList.ON_RELEASE);
         this.uiApi.addComponentHook(this.btn_warnWhenFriendIsOnline,ComponentHookList.ON_RELEASE);
         this.uiApi.addComponentHook(this.btn_shareStatus,ComponentHookList.ON_RELEASE);
         this.uiApi.addComponentHook(this.btn_friend,ComponentHookList.ON_RELEASE);
         this.uiApi.addComponentHook(this.btn_contact,ComponentHookList.ON_RELEASE);
         this.uiApi.addComponentHook(this.btn_enemy,ComponentHookList.ON_RELEASE);
         this.uiApi.addComponentHook(this.btn_ignored,ComponentHookList.ON_RELEASE);
         this.uiApi.addShortcutHook("validUi",this.onShortcut);
         this._iconsPath = this.uiApi.me().getConstant("icons_uri");
         this._bgLevelUri = this.uiApi.createUri(this.uiApi.me().getConstant("bgLevel_uri"));
         this._bgPrestigeUri = this.uiApi.createUri(this.uiApi.me().getConstant("bgPrestige_uri"));
         this._friendsList = this.socialApi.getFriendsList();
         this._contactsList = this.socialApi.getContactsList();
         this._enemiesList = this.socialApi.getEnemiesList();
         this._ignoredList = this.socialApi.getIgnoredList();
         var hasRights:Boolean = Boolean(this.sysApi.getPlayerManager().hasRights);
         if(!hasRights)
         {
            this.inp_add.restrict = "^ ";
         }
         this.uiApi.setRadioGroupSelectedItem("tabHGroup",this.btn_friend,this.uiApi.me());
         this.btn_friend.selected = true;
         this.currentTabName = this.btn_friend.name;
         this.btn_showOfflineFriends.selected = false;
         this.btn_warnWhenFriendIsOnline.selected = this.socialApi.getWarnOnFriendConnec();
         this.btn_shareStatus.selected = !this.socialApi.getShareStatus();
         this.displaySelectedTab(this._nCurrentTab);
      }
      
      public function updateFriendLine(data:*, components:*, selected:Boolean) : void
      {
         var playerLink:String = null;
         var icon:EmblemSymbol = null;
         var ariw:ArenaRankInfosWrapper = null;
         var smiley:Smiley = null;
         if(!this._interactiveComponentsList[components.btn_delete.name])
         {
            this.uiApi.addComponentHook(components.btn_delete,ComponentHookList.ON_RELEASE);
            this.uiApi.addComponentHook(components.btn_delete,ComponentHookList.ON_ROLL_OVER);
            this.uiApi.addComponentHook(components.btn_delete,ComponentHookList.ON_ROLL_OUT);
         }
         this._interactiveComponentsList[components.btn_delete.name] = data;
         if(!this._interactiveComponentsList[components.lbl_name.name])
         {
            this.uiApi.addComponentHook(components.lbl_name,ComponentHookList.ON_ROLL_OVER);
            this.uiApi.addComponentHook(components.lbl_name,ComponentHookList.ON_ROLL_OUT);
         }
         this._interactiveComponentsList[components.lbl_name.name] = data;
         if(!this._interactiveComponentsList[components.tx_status.name])
         {
            this.uiApi.addComponentHook(components.tx_status,ComponentHookList.ON_ROLL_OVER);
            this.uiApi.addComponentHook(components.tx_status,ComponentHookList.ON_ROLL_OUT);
         }
         this._interactiveComponentsList[components.tx_status.name] = data;
         if(!this._interactiveComponentsList[components.tx_state.name])
         {
            this.uiApi.addComponentHook(components.tx_state,ComponentHookList.ON_ROLL_OVER);
            this.uiApi.addComponentHook(components.tx_state,ComponentHookList.ON_ROLL_OUT);
            this.uiApi.addComponentHook(components.tx_state,ComponentHookList.ON_RELEASE);
         }
         this._interactiveComponentsList[components.tx_state.name] = data;
         if(!this._interactiveComponentsList[components.tx_mood.name])
         {
            this.uiApi.addComponentHook(components.tx_mood,ComponentHookList.ON_ROLL_OVER);
            this.uiApi.addComponentHook(components.tx_mood,ComponentHookList.ON_ROLL_OUT);
            this.uiApi.addComponentHook(components.tx_mood,ComponentHookList.ON_RELEASE);
         }
         this._interactiveComponentsList[components.tx_mood.name] = data;
         if(!this._interactiveComponentsList[components.tx_fight.name])
         {
            this.uiApi.addComponentHook(components.tx_fight,ComponentHookList.ON_ROLL_OVER);
            this.uiApi.addComponentHook(components.tx_fight,ComponentHookList.ON_ROLL_OUT);
            this.uiApi.addComponentHook(components.tx_fight,ComponentHookList.ON_RELEASE);
         }
         this._interactiveComponentsList[components.tx_fight.name] = data;
         if(!this._interactiveComponentsList[components.tx_league.name])
         {
            this.uiApi.addComponentHook(components.tx_league,ComponentHookList.ON_ROLL_OVER);
            this.uiApi.addComponentHook(components.tx_league,ComponentHookList.ON_ROLL_OUT);
         }
         this._interactiveComponentsList[components.tx_league.name] = data;
         if(!this._interactiveComponentsList[components.tx_havenbag.name])
         {
            this.uiApi.addComponentHook(components.tx_havenbag,ComponentHookList.ON_ROLL_OVER);
            this.uiApi.addComponentHook(components.tx_havenbag,ComponentHookList.ON_ROLL_OUT);
            this.uiApi.addComponentHook(components.tx_havenbag,ComponentHookList.ON_RELEASE);
         }
         this._interactiveComponentsList[components.tx_havenbag.name] = data;
         if(data)
         {
            components.lbl_name.text = "{account," + data.name + "," + data.accountId + "::" + data.name + "}";
            components.btn_delete.visible = true;
            components.tx_mood.uri = null;
            components.tx_fight.uri = null;
            components.tx_status.uri = null;
            components.tx_emblemBackGuild.uri = null;
            components.tx_emblemUpGuild.uri = null;
            components.tx_level.uri = null;
            components.tx_havenbag.visible = false;
            if(data.type == "Ignored")
            {
               components.lbl_level.text = "";
               components.lbl_guild.text = "";
               components.lbl_achievement.text = "";
               components.tx_league.uri = null;
               components.tx_state.uri = null;
               components.tx_head.uri = null;
            }
            else if(!data.online)
            {
               components.lbl_level.text = "-";
               components.lbl_guild.text = "-";
               components.lbl_achievement.text = "-";
               components.tx_league.uri = null;
               components.tx_state.uri = null;
               components.tx_head.uri = null;
            }
            else
            {
               if(data.playerId)
               {
                  playerLink = "{player," + data.playerName + "," + data.playerId + "::" + data.name + " (" + data.playerName + ")}";
               }
               else
               {
                  playerLink = "{player," + data.playerName + "::" + data.name + " (" + data.playerName + ")}";
               }
               components.lbl_name.text = playerLink;
               components.lbl_level.text = "?";
               if(data.type != "Contact")
               {
                  components.tx_head.uri = this.uiApi.createUri(this.uiApi.me().getConstant("heads") + data.breed + "" + data.sex + ".png");
               }
               else
               {
                  components.tx_head.uri = null;
               }
               if(data.type == "Enemy")
               {
                  components.lbl_guild.text = "?";
                  components.lbl_achievement.text = "?";
                  components.tx_league.uri = null;
                  components.tx_state.uri = null;
               }
               else
               {
                  if(data.level > 0)
                  {
                     if(data.level > ProtocolConstantsEnum.MAX_LEVEL)
                     {
                        components.lbl_level.cssClass = "darkboldcenter";
                        components.lbl_level.text = "" + (data.level - ProtocolConstantsEnum.MAX_LEVEL);
                        components.tx_level.uri = this._bgPrestigeUri;
                        this.uiApi.addComponentHook(components.tx_level,ComponentHookList.ON_ROLL_OVER);
                        this.uiApi.addComponentHook(components.tx_level,ComponentHookList.ON_ROLL_OUT);
                     }
                     else
                     {
                        this.uiApi.removeComponentHook(components.tx_level,ComponentHookList.ON_ROLL_OVER);
                        this.uiApi.removeComponentHook(components.tx_level,ComponentHookList.ON_ROLL_OUT);
                        components.lbl_level.cssClass = "boldcenter";
                        components.lbl_level.text = data.level;
                        components.tx_level.uri = this._bgLevelUri;
                     }
                  }
                  if(data.realGuildName == "")
                  {
                     components.lbl_guild.text = "";
                  }
                  else
                  {
                     components.lbl_guild.text = this.chatApi.getGuildLink(data,data.guildName);
                  }
                  if(Boolean(data.guildBackEmblem) && data.guildBackEmblem.idEmblem != 0)
                  {
                     components.tx_emblemBackGuild.uri = data.guildBackEmblem.iconUri;
                     components.tx_emblemUpGuild.uri = data.guildUpEmblem.iconUri;
                     this.utilApi.changeColor(components.tx_emblemBackGuild,data.guildBackEmblem.color,1);
                     icon = this.dataApi.getEmblemSymbol(data.guildUpEmblem.idEmblem);
                     if(Boolean(icon) && icon.colorizable)
                     {
                        this.utilApi.changeColor(components.tx_emblemUpGuild,data.guildUpEmblem.color,0);
                     }
                     else
                     {
                        this.utilApi.changeColor(components.tx_emblemUpGuild,data.guildUpEmblem.color,0,true);
                     }
                  }
                  if(data.achievementPoints == -1)
                  {
                     components.lbl_achievement.text = "-";
                  }
                  else
                  {
                     components.lbl_achievement.text = data.achievementPoints;
                  }
                  if(data.leagueId <= 0)
                  {
                     ariw = new ArenaRankInfosWrapper();
                     components.tx_league.uri = this.uiApi.createUri(this.uiApi.me().getConstant("leagues_uri") + SOCIAL_LEAGUE_ICON_PREFIX + ariw.LEAGUE_DEFAULT_ICON_ID);
                  }
                  else
                  {
                     components.tx_league.uri = this.uiApi.createUri(this.uiApi.me().getConstant("leagues_uri") + SOCIAL_LEAGUE_ICON_PREFIX + this.dataApi.getArenaLeagueById(data.leagueId).iconWithExtension);
                  }
                  if(data.havenbagShared)
                  {
                     components.tx_havenbag.visible = true;
                  }
                  else
                  {
                     components.tx_havenbag.visible = false;
                  }
                  if(data.statusId)
                  {
                     switch(data.statusId)
                     {
                        case PlayerStatusEnum.PLAYER_STATUS_AVAILABLE:
                           components.tx_status.uri = this.uiApi.createUri(this._iconsPath + "green.png");
                           break;
                        case PlayerStatusEnum.PLAYER_STATUS_AFK:
                        case PlayerStatusEnum.PLAYER_STATUS_IDLE:
                           components.tx_status.uri = this.uiApi.createUri(this._iconsPath + "yellow.png");
                           break;
                        case PlayerStatusEnum.PLAYER_STATUS_PRIVATE:
                           components.tx_status.uri = this.uiApi.createUri(this._iconsPath + "blue.png");
                           break;
                        case PlayerStatusEnum.PLAYER_STATUS_SOLO:
                           components.tx_status.uri = this.uiApi.createUri(this._iconsPath + "red.png");
                     }
                  }
                  if(data.moodSmileyId != 0)
                  {
                     smiley = this.dataApi.getSmiley(data.moodSmileyId);
                     if(smiley)
                     {
                        components.tx_mood.uri = this.uiApi.createUri(this.uiApi.me().getConstant("smilies_uri") + smiley.gfxId);
                     }
                  }
                  if(data.state == PlayerStateEnum.GAME_TYPE_FIGHT)
                  {
                     if(data.moodSmileyId == 0)
                     {
                        components.tx_state.uri = this.uiApi.createUri(this.uiApi.me().getConstant("assets") + "Social_tx_fightState");
                     }
                     else
                     {
                        components.tx_state.uri = null;
                        components.tx_fight.uri = this.uiApi.createUri(this.uiApi.me().getConstant("assets") + "Social_tx_fightState_small");
                     }
                  }
                  else
                  {
                     components.tx_state.uri = null;
                  }
               }
            }
         }
         else
         {
            components.lbl_name.text = "";
            components.tx_level.uri = null;
            components.lbl_level.text = "";
            components.lbl_guild.text = "";
            components.lbl_achievement.text = "";
            components.tx_league.uri = null;
            components.tx_emblemBackGuild.uri = null;
            components.tx_emblemUpGuild.uri = null;
            components.tx_state.uri = null;
            components.tx_fight.uri = null;
            components.tx_mood.uri = null;
            components.tx_head.uri = null;
            components.btn_delete.visible = false;
            components.tx_status.uri = null;
            components.tx_havenbag.visible = false;
         }
      }
      
      private function displaySelectedTab(tab:uint) : void
      {
         this.uiApi.getUi("socialBase").uiClass.setSubTab(tab);
         switch(tab)
         {
            case 0:
               if(!this.btn_showOfflineFriends.visible)
               {
                  this.btn_showOfflineFriends.visible = true;
               }
               if(!this.btn_warnWhenFriendIsOnline.visible)
               {
                  this.btn_warnWhenFriendIsOnline.visible = true;
               }
               if(this.btn_shareStatus.visible)
               {
                  this.btn_shareStatus.visible = false;
               }
               break;
            case 1:
               if(!this.btn_showOfflineFriends.visible)
               {
                  this.btn_showOfflineFriends.visible = true;
               }
               if(this.btn_warnWhenFriendIsOnline.visible)
               {
                  this.btn_warnWhenFriendIsOnline.visible = false;
               }
               if(!this.btn_shareStatus.visible)
               {
                  this.btn_shareStatus.visible = true;
               }
               break;
            case 2:
               if(this.btn_showOfflineFriends.visible)
               {
                  this.btn_showOfflineFriends.visible = false;
               }
               if(this.btn_warnWhenFriendIsOnline.visible)
               {
                  this.btn_warnWhenFriendIsOnline.visible = false;
               }
               if(this.btn_shareStatus.visible)
               {
                  this.btn_shareStatus.visible = false;
               }
               break;
            case 3:
               this.btn_showOfflineFriends.visible = false;
               if(this.btn_warnWhenFriendIsOnline.visible)
               {
                  this.btn_warnWhenFriendIsOnline.visible = false;
               }
               if(this.btn_shareStatus.visible)
               {
                  this.btn_shareStatus.visible = false;
               }
         }
         this.refreshGrid(tab);
      }
      
      private function refreshGrid(tabToRefresh:int = -1) : void
      {
         var datas:Object = null;
         var onlyOnlinePeople:Array = null;
         var guy:* = undefined;
         if(tabToRefresh == -1)
         {
            tabToRefresh = int(this._nCurrentTab);
         }
         if(tabToRefresh == this._nCurrentTab)
         {
            datas = new Object();
            switch(this._nCurrentTab)
            {
               case 0:
                  datas = this._friendsList;
                  break;
               case 1:
                  datas = this._contactsList;
                  break;
               case 2:
                  datas = this._enemiesList;
                  break;
               case 3:
                  datas = this._ignoredList;
            }
            if(!this._bShowOfflineGuys && (this._nCurrentTab == 0 || this._nCurrentTab == 1))
            {
               onlyOnlinePeople = new Array();
               for each(guy in datas)
               {
                  if(guy.online)
                  {
                     onlyOnlinePeople.push(guy);
                  }
               }
               this.gd_list.dataProvider = onlyOnlinePeople;
            }
            else
            {
               this.gd_list.dataProvider = datas;
            }
         }
      }
      
      private function onFriendsUpdated() : void
      {
         this._friendsList = this.socialApi.getFriendsList();
         this.refreshGrid(0);
      }
      
      private function onContactsUpdated() : void
      {
         this._contactsList = this.socialApi.getContactsList();
         this.refreshGrid(1);
      }
      
      private function onEnemiesUpdated() : void
      {
         this._enemiesList = this.socialApi.getEnemiesList();
         this.refreshGrid(2);
      }
      
      private function onIgnoredUpdated() : void
      {
         this._ignoredList = this.socialApi.getIgnoredList();
         this.refreshGrid(3);
      }
      
      private function onFriendWarningState(enable:Boolean) : void
      {
         if(this.btn_warnWhenFriendIsOnline.selected != enable)
         {
            this.btn_warnWhenFriendIsOnline.selected = enable;
         }
      }
      
      private function onStatusShareState(enable:Boolean) : void
      {
         if(this.btn_shareStatus.selected == enable)
         {
            this.btn_shareStatus.selected = !enable;
         }
      }
      
      public function onShortcut(s:String) : Boolean
      {
         switch(s)
         {
            case "validUi":
               if(this.inp_add.haveFocus && this.inp_add.text != "")
               {
                  switch(this._nCurrentTab)
                  {
                     case 0:
                        this.sysApi.sendAction(new AddFriend(this.inp_add.text));
                        break;
                     case 1:
                        this.sysApi.sendAction(new AddFriend(this.inp_add.text));
                        break;
                     case 2:
                        this.sysApi.sendAction(new AddEnemy(this.inp_add.text));
                        break;
                     case 3:
                        this.sysApi.sendAction(new AddIgnored(this.inp_add.text));
                  }
                  this.inp_add.text = "";
                  return true;
               }
         }
         return false;
      }
      
      public function selectWhichTabHintsToDisplay() : void
      {
         this.hintsApi.showSubHints(this.currentTabName);
      }
      
      public function onRelease(target:Object) : void
      {
         var data:Object = null;
         var name:String = null;
         switch(target)
         {
            case this.btn_add:
               switch(this._nCurrentTab)
               {
                  case 0:
                     if(this.inp_add.text)
                     {
                        this.sysApi.sendAction(new AddFriend(this.inp_add.text));
                     }
                     break;
                  case 1:
                     if(this.inp_add.text)
                     {
                        this.sysApi.sendAction(new AddFriend(this.inp_add.text));
                     }
                     break;
                  case 2:
                     if(this.inp_add.text)
                     {
                        this.sysApi.sendAction(new AddEnemy(this.inp_add.text));
                     }
                     break;
                  case 3:
                     if(this.inp_add.text)
                     {
                        this.sysApi.sendAction(new AddIgnored(this.inp_add.text));
                     }
               }
               this.lbl_add.visible = true;
               this.inp_add.text = "";
               break;
            case this.btn_friend:
               if(this._nCurrentTab != 0)
               {
                  this._nCurrentTab = 0;
                  this.displaySelectedTab(this._nCurrentTab);
                  this.currentTabName = target.name;
                  this.hintsApi.uiTutoTabLaunch();
               }
               break;
            case this.btn_contact:
               if(this._nCurrentTab != 1)
               {
                  this._nCurrentTab = 1;
                  this.displaySelectedTab(this._nCurrentTab);
                  this.currentTabName = target.name;
                  this.hintsApi.uiTutoTabLaunch();
               }
               break;
            case this.btn_enemy:
               if(this._nCurrentTab != 2)
               {
                  this._nCurrentTab = 2;
                  this.displaySelectedTab(this._nCurrentTab);
                  this.currentTabName = target.name;
                  this.hintsApi.uiTutoTabLaunch();
               }
               break;
            case this.btn_ignored:
               if(this._nCurrentTab != 3)
               {
                  this._nCurrentTab = 3;
                  this.displaySelectedTab(this._nCurrentTab);
                  this.currentTabName = target.name;
                  this.hintsApi.uiTutoTabLaunch();
               }
               break;
            case this.btn_showOfflineFriends:
               this._bShowOfflineGuys = this.btn_showOfflineFriends.selected;
               this.refreshGrid(this._nCurrentTab);
               break;
            case this.btn_warnWhenFriendIsOnline:
               this.sysApi.sendAction(new FriendWarningSet(this.btn_warnWhenFriendIsOnline.selected));
               break;
            case this.btn_shareStatus:
               this.sysApi.sendAction(new StatusShareSet(!this.btn_shareStatus.selected));
               break;
            case this.btn_tabGuild:
               if(this._bDescendingSort)
               {
                  this.gd_list.sortOn("guildName",Array.CASEINSENSITIVE);
               }
               else
               {
                  this.gd_list.sortOn("guildName",Array.CASEINSENSITIVE | Array.DESCENDING);
               }
               this._bDescendingSort = !this._bDescendingSort;
               break;
            case this.btn_tabName:
               if(this._bDescendingSort)
               {
                  this.gd_list.sortOn("name",Array.CASEINSENSITIVE);
               }
               else
               {
                  this.gd_list.sortOn("name",Array.CASEINSENSITIVE | Array.DESCENDING);
               }
               this._bDescendingSort = !this._bDescendingSort;
               break;
            case this.btn_tabLevel:
               if(this._bDescendingSort)
               {
                  this.gd_list.sortOn("level",Array.NUMERIC);
               }
               else
               {
                  this.gd_list.sortOn("level",Array.NUMERIC | Array.DESCENDING);
               }
               this._bDescendingSort = !this._bDescendingSort;
               break;
            case this.btn_tabBreed:
               if(this._bDescendingSort)
               {
                  this.gd_list.sortOn("breed",Array.NUMERIC);
               }
               else
               {
                  this.gd_list.sortOn("breed",Array.NUMERIC | Array.DESCENDING);
               }
               this._bDescendingSort = !this._bDescendingSort;
               break;
            case this.btn_tabAchievement:
               if(this._bDescendingSort)
               {
                  this.gd_list.sortOn("achievementPoints",Array.NUMERIC);
               }
               else
               {
                  this.gd_list.sortOn("achievementPoints",Array.NUMERIC | Array.DESCENDING);
               }
               this._bDescendingSort = !this._bDescendingSort;
               break;
            case this.btn_tabLeague:
               if(this._bDescendingSort)
               {
                  this.gd_list.sortOn("leagueId",Array.NUMERIC);
               }
               else
               {
                  this.gd_list.sortOn("leagueId",Array.NUMERIC | Array.DESCENDING);
               }
               this._bDescendingSort = !this._bDescendingSort;
               break;
            case this.btn_tabState:
               if(this._bDescendingSort)
               {
                  this.gd_list.sortOn("state",Array.NUMERIC);
               }
               else
               {
                  this.gd_list.sortOn("state",Array.NUMERIC | Array.DESCENDING);
               }
               this._bDescendingSort = !this._bDescendingSort;
               break;
            default:
               if(target.name.indexOf("btn_delete") != -1)
               {
                  data = this._interactiveComponentsList[target.name];
                  if(data.type == "Friend" || data.type == "Contact")
                  {
                     if(this._friendIdWaitingForKick == -1)
                     {
                        if(data.online)
                        {
                           name = data.name + " ( " + data.playerName + " )";
                        }
                        else
                        {
                           name = data.name;
                        }
                        this._friendIdWaitingForKick = data.accountId;
                        this.modCommon.openPopup(this.uiApi.getText("ui.popup.warning"),[this.uiApi.getText("ui.social.doUDeleteFriend",name)],[this.uiApi.getText("ui.common.yes"),this.uiApi.getText("ui.common.no")],[this.onConfirmDeleteFriend,this.onCancelDeleteFriend],this.onConfirmDeleteFriend,this.onCancelDeleteFriend);
                     }
                  }
                  else if(data.type == "Enemy")
                  {
                     if(this._enemyIdWaitingForKick == -1)
                     {
                        if(data.online)
                        {
                           name = data.name + " ( " + data.playerName + " )";
                        }
                        else
                        {
                           name = data.name;
                        }
                        this._enemyIdWaitingForKick = data.accountId;
                        this.modCommon.openPopup(this.uiApi.getText("ui.popup.warning"),[this.uiApi.getText("ui.social.doUDeleteEnemy",name)],[this.uiApi.getText("ui.common.yes"),this.uiApi.getText("ui.common.no")],[this.onConfirmDeleteEnemy,this.onCancelDeleteEnemy],this.onConfirmDeleteEnemy,this.onCancelDeleteEnemy);
                     }
                  }
                  else if(data.type == "Ignored")
                  {
                     this.sysApi.sendAction(new RemoveIgnored(data.accountId));
                  }
               }
               else if(target.name.indexOf("tx_state") != -1 || target.name.indexOf("tx_mood") != -1 || target.name.indexOf("tx_fight") != -1)
               {
                  data = this._interactiveComponentsList[target.name];
                  if(data.state == PlayerStateEnum.GAME_TYPE_FIGHT)
                  {
                     this.sysApi.sendAction(new GameFightSpectatePlayerRequest(data.playerId));
                  }
               }
               else if(target.name.indexOf("tx_havenbag") != -1)
               {
                  data = this._interactiveComponentsList[target.name];
                  this.sysApi.sendAction(new HavenbagEnter(data.playerId));
               }
         }
      }
      
      public function onRollOver(target:Object) : void
      {
         var text:String = null;
         var data:Object = null;
         var league:ArenaLeague = null;
         var months:int = 0;
         var days:int = 0;
         var argText:String = null;
         var sdata:Object = null;
         var point:uint = LocationEnum.POINT_BOTTOM;
         var relPoint:uint = LocationEnum.POINT_TOP;
         if(target.name.indexOf("btn_delete") != -1)
         {
            text = this.uiApi.getText("ui.charsel.characterDelete");
         }
         else if(target.name.indexOf("tx_state") != -1 || target.name.indexOf("tx_mood") != -1 || target.name.indexOf("tx_fight") != -1)
         {
            data = this._interactiveComponentsList[target.name];
            if(data.state == PlayerStateEnum.GAME_TYPE_FIGHT)
            {
               text = this.uiApi.getText("ui.spectator.clicToJoin");
            }
         }
         else if(target.name.indexOf("tx_havenbag") != -1)
         {
            text = this.uiApi.getText("ui.havenbag.visit");
         }
         else if(target.name.indexOf("tx_league") != -1)
         {
            league = this.dataApi.getArenaLeagueById(this._interactiveComponentsList[target.name].leagueId);
            if(league)
            {
               text = league.name;
               if(league.isLastLeague && this._interactiveComponentsList[target.name].ladderPosition > 0)
               {
                  text += " " + this._interactiveComponentsList[target.name].ladderPosition;
               }
            }
            else
            {
               text = this.uiApi.getText("ui.party.arena.noLeague");
            }
         }
         else if(target.name.indexOf("lbl_name") != -1)
         {
            data = this._interactiveComponentsList[target.name];
            if(Boolean(data) && !data.online)
            {
               if(data.lastConnection > 0)
               {
                  months = Math.floor(data.lastConnection / 720);
                  days = (data.lastConnection - months * 720) / 24;
                  if(months > 0)
                  {
                     if(days > 0)
                     {
                        argText = this.uiApi.processText(this.uiApi.getText("ui.social.monthsAndDaysSinceLastConnection",months,days),"m",days <= 1);
                     }
                     else
                     {
                        argText = this.uiApi.processText(this.uiApi.getText("ui.social.monthsSinceLastConnection",months),"m",months <= 1);
                     }
                  }
                  else if(days > 0)
                  {
                     argText = this.uiApi.processText(this.uiApi.getText("ui.social.daysSinceLastConnection",days),"m",days <= 1);
                  }
                  else
                  {
                     argText = this.uiApi.getText("ui.social.lessThanADay");
                  }
                  text = this.uiApi.getText("ui.social.lastConnection",argText);
               }
               else
               {
                  text = this.uiApi.getText("ui.social.unknownLastConnection");
               }
               relPoint = 0;
            }
         }
         else if(target.name.indexOf("tx_status") != -1)
         {
            sdata = this._interactiveComponentsList[target.name];
            if(Boolean(sdata) && Boolean(sdata.online))
            {
               switch(sdata.statusId)
               {
                  case PlayerStatusEnum.PLAYER_STATUS_AVAILABLE:
                     text = this.uiApi.getText("ui.chat.status.availiable");
                     break;
                  case PlayerStatusEnum.PLAYER_STATUS_AFK:
                     text = this.uiApi.getText("ui.chat.status.away");
                     if(sdata.awayMessage != "")
                     {
                        text += this.uiApi.getText("ui.common.colon") + sdata.awayMessage;
                     }
                     break;
                  case PlayerStatusEnum.PLAYER_STATUS_IDLE:
                     text = this.uiApi.getText("ui.chat.status.idle");
                     break;
                  case PlayerStatusEnum.PLAYER_STATUS_PRIVATE:
                     text = this.uiApi.getText("ui.chat.status.private");
                     break;
                  case PlayerStatusEnum.PLAYER_STATUS_SOLO:
                     text = this.uiApi.getText("ui.chat.status.solo");
               }
            }
         }
         else if(target.name.indexOf("tx_level") != -1)
         {
            text = this.uiApi.getText("ui.tooltip.OmegaLevel");
         }
         if(text)
         {
            this.uiApi.showTooltip(this.uiApi.textTooltipInfo(text),target,false,"standard",point,relPoint,0,null,null,null,"TextInfo");
         }
      }
      
      public function onRollOut(target:Object) : void
      {
         this.uiApi.hideTooltip();
      }
      
      public function onFocusChange(target:Object) : void
      {
         if(Boolean(target) && target == this.inp_add)
         {
            this.lbl_add.visible = false;
         }
         else if(target != this.inp_add && this.inp_add.text == "")
         {
            this.lbl_add.visible = true;
         }
      }
      
      private function onConfirmDeleteFriend() : void
      {
         this.sysApi.sendAction(new RemoveFriend(this._friendIdWaitingForKick));
         this._friendIdWaitingForKick = -1;
      }
      
      private function onCancelDeleteFriend() : void
      {
         this._friendIdWaitingForKick = -1;
      }
      
      private function onConfirmDeleteEnemy() : void
      {
         this.sysApi.sendAction(new RemoveEnemy(this._enemyIdWaitingForKick));
         this._enemyIdWaitingForKick = -1;
      }
      
      private function onCancelDeleteEnemy() : void
      {
         this._enemyIdWaitingForKick = -1;
      }
   }
}

