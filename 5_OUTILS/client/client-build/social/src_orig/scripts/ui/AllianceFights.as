package ui
{
   import com.ankamagames.berilia.api.UiApi;
   import com.ankamagames.berilia.components.Grid;
   import com.ankamagames.berilia.components.Label;
   import com.ankamagames.dofus.datacenter.breeds.Breed;
   import com.ankamagames.dofus.internalDatacenter.guild.SocialEntityInFightWrapper;
   import com.ankamagames.dofus.internalDatacenter.guild.SocialFightersWrapper;
   import com.ankamagames.dofus.network.types.game.character.CharacterMinimalAllianceInformations;
   import com.ankamagames.dofus.network.types.game.character.CharacterMinimalGuildInformations;
   import com.ankamagames.dofus.network.types.game.character.CharacterMinimalPlusLookInformations;
   import com.ankamagames.dofus.uiApi.DataApi;
   import com.ankamagames.dofus.uiApi.PlayedCharacterApi;
   import com.ankamagames.dofus.uiApi.SocialApi;
   import com.ankamagames.dofus.uiApi.SystemApi;
   import com.ankamagames.dofus.uiApi.UiTutoApi;
   import com.ankamagames.dofus.uiApi.UtilApi;
   import d2actions.*;
   import d2enums.ComponentHookList;
   import d2enums.GuildInformationsTypeEnum;
   import d2enums.ProtocolConstantsEnum;
   import d2enums.SelectMethodEnum;
   import d2enums.TaxCollectorStateEnum;
   import d2hooks.*;
   import flash.utils.Dictionary;
   import flash.utils.getTimer;
   import ui.data.SocialEntityDisplayInfo;
   import utils.JoinFightUtil;
   
   public class AllianceFights
   {
      
      private static var _self:AllianceFights;
      
      private static const TYPE_TAX_COLLECTOR:int = 0;
      
      private static const TYPE_PRISM:int = 1;
      
      public var sysApi:SystemApi;
      
      public var uiApi:UiApi;
      
      public var hintsApi:UiTutoApi;
      
      [Module(name="Ankama_Common")]
      public var modCommon:Object;
      
      public var socialApi:SocialApi;
      
      public var playerApi:PlayedCharacterApi;
      
      public var utilApi:UtilApi;
      
      public var dataApi:DataApi;
      
      private var _forceShowType:int = -1;
      
      private var _forceShowFight:int = -1;
      
      private var _interactiveComponentsList:Dictionary = new Dictionary(true);
      
      private var _fightsList:Array = new Array();
      
      private var _defenders:Dictionary = new Dictionary(true);
      
      private var _attackers:Dictionary = new Dictionary(true);
      
      private var _fightsInfoForDisplay:Dictionary = new Dictionary(true);
      
      private var _assetsPath:String;
      
      private var _prismText:String;
      
      private var _levelText:String;
      
      private var _prestigeText:String;
      
      private var _fightsTimerInfos:Array = new Array();
      
      public var gd_fights:Grid;
      
      public var lbl_fightsCount:Label;
      
      public function AllianceFights()
      {
         super();
      }
      
      public static function getInstance() : AllianceFights
      {
         return _self;
      }
      
      public function main(... args) : void
      {
         var fight:SocialEntityInFightWrapper = null;
         _self = this;
         this.sysApi.addHook(TaxCollectorListUpdate,this.onTaxCollectorsListUpdate);
         this.sysApi.addHook(TaxCollectorUpdate,this.onTaxCollectorUpdate);
         this.sysApi.addHook(AllianceTaxCollectorRemoved,this.onAllianceTaxCollectorRemoved);
         this.sysApi.addHook(PrismsInFightList,this.onPrismsInFightList);
         this.sysApi.addHook(PrismInFightAdded,this.onPrismInFightAdded);
         this.sysApi.addHook(PrismInFightRemoved,this.onPrismInFightRemoved);
         this.sysApi.addHook(GuildFightEnnemiesListUpdate,this.onGuildFightEnnemiesListUpdate);
         this.sysApi.addHook(GuildFightAlliesListUpdate,this.onGuildFightAlliesListUpdate);
         this.sysApi.sendAction(new GuildGetInformations(GuildInformationsTypeEnum.INFO_TAX_COLLECTOR_ALLIANCE));
         if(Boolean(args) && Boolean(args[0]))
         {
            this._forceShowType = args[0][0];
            this._forceShowFight = args[0][1];
         }
         else
         {
            this._forceShowType = -1;
            this._forceShowFight = -1;
         }
         this._assetsPath = this.uiApi.me().getConstant("texture");
         this._prismText = this.uiApi.getText("ui.zaap.prism");
         this._levelText = this.uiApi.getText("ui.common.short.level");
         this._prestigeText = this.uiApi.getText("ui.common.short.prestige");
         for each(fight in this.socialApi.getAllFightingTaxCollectors())
         {
            this._fightsList.push(fight);
            this.prepareOneFightData(fight);
         }
         for each(fight in this.socialApi.getFightingPrisms())
         {
            this._fightsList.push(fight);
            this.prepareOneFightData(fight);
         }
         this.refreshGrid();
      }
      
      public function unload() : void
      {
         this.sysApi.removeEventListener(this.onEnterFrame);
      }
      
      public function showTabHints() : void
      {
         this.hintsApi.showSubHints();
      }
      
      public function updateGridLine(data:*, components:*, selected:Boolean) : void
      {
         var info:SocialEntityDisplayInfo = null;
         if(!this._interactiveComponentsList[components.gd_defenseTeam.name])
         {
            this.uiApi.addComponentHook(components.gd_defenseTeam,ComponentHookList.ON_SELECT_ITEM);
            this.uiApi.addComponentHook(components.gd_defenseTeam,"onSelectEmptyItem");
            this.uiApi.addComponentHook(components.gd_defenseTeam,ComponentHookList.ON_ITEM_ROLL_OVER);
            this.uiApi.addComponentHook(components.gd_defenseTeam,ComponentHookList.ON_ITEM_ROLL_OUT);
         }
         this._interactiveComponentsList[components.gd_defenseTeam.name] = data;
         if(!this._interactiveComponentsList[components.gd_attackTeam.name])
         {
            this.uiApi.addComponentHook(components.gd_attackTeam,ComponentHookList.ON_ITEM_ROLL_OVER);
            this.uiApi.addComponentHook(components.gd_attackTeam,ComponentHookList.ON_ITEM_ROLL_OUT);
         }
         this._interactiveComponentsList[components.gd_attackTeam.name] = data;
         if(!this._interactiveComponentsList[components.tx_attackTeam.name])
         {
            this.uiApi.addComponentHook(components.tx_attackTeam,ComponentHookList.ON_ROLL_OVER);
            this.uiApi.addComponentHook(components.tx_attackTeam,ComponentHookList.ON_ROLL_OUT);
         }
         this._interactiveComponentsList[components.tx_attackTeam.name] = data;
         if(!this._interactiveComponentsList[components.tx_defenseTeam.name])
         {
            this.uiApi.addComponentHook(components.tx_defenseTeam,ComponentHookList.ON_ROLL_OVER);
            this.uiApi.addComponentHook(components.tx_defenseTeam,ComponentHookList.ON_ROLL_OUT);
         }
         this._interactiveComponentsList[components.tx_defenseTeam.name] = data;
         if(!this._interactiveComponentsList[components.tx_spectate.name])
         {
            this.uiApi.addComponentHook(components.tx_spectate,ComponentHookList.ON_RELEASE);
            this.uiApi.addComponentHook(components.tx_spectate,ComponentHookList.ON_ROLL_OVER);
            this.uiApi.addComponentHook(components.tx_spectate,ComponentHookList.ON_ROLL_OUT);
         }
         this._interactiveComponentsList[components.tx_spectate.name] = data;
         if(data)
         {
            info = this._fightsInfoForDisplay[data.uniqueId];
            if(this._forceShowType == data.typeId && this._forceShowFight == data.uniqueId)
            {
               components.ctr_line.bgColor = this.sysApi.getConfigEntry("colors.grid.over");
            }
            else
            {
               components.ctr_line.bgColor = -1;
            }
            components.lbl_name.text = info.name;
            components.lbl_loc.text = info.location;
            if(data.typeId == TYPE_TAX_COLLECTOR)
            {
               components.lbl_infos.text = info.guildName;
            }
            else
            {
               components.lbl_infos.text = info.birthDate;
            }
            components.tx_attackTeam.visible = true;
            components.tx_defenseTeam.visible = true;
            components.tx_spectate.visible = true;
            components.gd_attackTeam.dataProvider = this._attackers[data.uniqueId].charactersInfoList;
            components.gd_defenseTeam.dataProvider = this._defenders[data.uniqueId].charactersInfoList;
            components.gd_attackTeam.visible = true;
            components.gd_defenseTeam.visible = true;
            if(info.elapsedTimePercent >= 1 || info.state == TaxCollectorStateEnum.STATE_FIGHTING)
            {
               components.pb_time.value = 0;
               components.pb_time.visible = false;
            }
            else
            {
               if(!components.pb_time.visible)
               {
                  components.pb_time.visible = true;
               }
               components.pb_time.value = info.elapsedTimePercent;
            }
            components.tx_fightType.uri = info.symbolUri;
         }
         else
         {
            components.ctr_line.bgColor = -1;
            components.tx_fightType.uri = null;
            components.pb_time.visible = false;
            components.tx_attackTeam.visible = false;
            components.tx_defenseTeam.visible = false;
            components.tx_spectate.visible = false;
            components.lbl_name.text = "";
            components.lbl_loc.text = "";
            components.lbl_infos.text = "";
            components.gd_attackTeam.visible = false;
            components.gd_defenseTeam.visible = false;
         }
      }
      
      private function refreshGrid() : void
      {
         var currentFightsCount:uint = this._fightsList.length;
         this.lbl_fightsCount.text = this.uiApi.processText(this.uiApi.getText("ui.alliance.currentFights",currentFightsCount),"m",currentFightsCount < 2);
         this._fightsList.sortOn("fightTime",Array.NUMERIC | Array.DESCENDING);
         this.gd_fights.dataProvider = this._fightsList;
         for(var i:int = 0; i < currentFightsCount; )
         {
            if(this._forceShowType == this._fightsList[i].typeId && this._forceShowFight == this._fightsList[i].uniqueId)
            {
               this.gd_fights.moveTo(i,true);
               return;
            }
            i++;
         }
      }
      
      private function refreshOneFight(fightId:Number) : void
      {
         var currentFightsCount:uint = this._fightsList.length;
         var i:int = 0;
         for(this.sysApi.log(2,"currentFightsCount = " + currentFightsCount); i <= currentFightsCount; )
         {
            this.sysApi.log(2,"_fightsList[" + i + "] is null ? " + this._fightsList[i] ? "false" : "true");
            if(Boolean(this._fightsList[i]) && this._fightsList[i].uniqueId == fightId)
            {
               this.gd_fights.updateItem(i);
               return;
            }
            i++;
         }
      }
      
      private function prepareOneFightData(data:SocialEntityInFightWrapper) : void
      {
         var uptodateState:int = 0;
         if(!this._fightsInfoForDisplay[data.uniqueId])
         {
            this._fightsInfoForDisplay[data.uniqueId] = new SocialEntityDisplayInfo(data);
         }
         var clock:uint = uint(getTimer());
         if(data.fightTime > clock)
         {
            uptodateState = int(TaxCollectorStateEnum.STATE_WAITING_FOR_HELP);
         }
         else
         {
            uptodateState = int(TaxCollectorStateEnum.STATE_FIGHTING);
         }
         this._fightsInfoForDisplay[data.uniqueId].stateUpdate(uptodateState);
         this._fightsInfoForDisplay[data.uniqueId].symbolUri = this.uiApi.createUri(this._assetsPath + this._fightsInfoForDisplay[data.uniqueId].symbolName);
         if(this._fightsInfoForDisplay[data.uniqueId].state == TaxCollectorStateEnum.STATE_WAITING_FOR_HELP)
         {
            this.addFightTimerInfo(data);
         }
         else
         {
            this.removeFightTimerInfo(data.uniqueId);
         }
         this.updateFightersLists(data);
      }
      
      private function removeOneFightData(fightId:Number) : void
      {
         this.removeFightTimerInfo(fightId);
         this._fightsInfoForDisplay[fightId] = null;
         this._defenders[fightId] = null;
         this._attackers[fightId] = null;
      }
      
      private function updateFightersLists(data:SocialEntityInFightWrapper) : void
      {
         var attackersText:String = null;
         var defendersText:String = null;
         var enemies:Object = null;
         var allies:Object = null;
         var e2:* = undefined;
         var a2:* = undefined;
         attackersText = this.uiApi.getText("ui.common.attackers") + " : \n";
         defendersText = this.uiApi.getText("ui.common.defenders") + " : \n";
         if(Boolean(data) && Boolean(data.enemyCharactersInformations) && data.enemyCharactersInformations.length > 0)
         {
            enemies = data.enemyCharactersInformations;
            for each(e2 in data.enemyCharactersInformations)
            {
               attackersText += this.getFighterName(e2.playerCharactersInformations) + "\n";
            }
         }
         if(Boolean(data) && Boolean(data.allyCharactersInformations) && data.allyCharactersInformations.length > 0)
         {
            allies = data.allyCharactersInformations;
            for each(a2 in data.allyCharactersInformations)
            {
               defendersText += this.getFighterName(a2.playerCharactersInformations) + "\n";
            }
         }
         this._attackers[data.uniqueId] = {
            "charactersInfoList":enemies,
            "charactersNameList":attackersText
         };
         this._defenders[data.uniqueId] = {
            "charactersInfoList":allies,
            "charactersNameList":defendersText
         };
      }
      
      private function addFightTimerInfo(data:SocialEntityInFightWrapper) : void
      {
         var timerInfo:Object = {
            "uniqueId":data.uniqueId,
            "clockEnd":data.fightTime,
            "clockDuration":data.waitTimeForPlacement
         };
         if(this._fightsTimerInfos.length == 0)
         {
            this.sysApi.addEventListener(this.onEnterFrame,"time");
         }
         this._fightsTimerInfos.push(timerInfo);
      }
      
      private function removeFightTimerInfo(uniqueId:Number) : void
      {
         var i:int = 0;
         for(var fightsCount:int = int(this._fightsTimerInfos.length); i < fightsCount; )
         {
            if(this._fightsTimerInfos[i].uniqueId == uniqueId)
            {
               this._fightsTimerInfos.splice(i,1);
               break;
            }
            i++;
         }
         if(this._fightsTimerInfos.length == 0)
         {
            this.sysApi.removeEventListener(this.onEnterFrame);
         }
      }
      
      private function getFighterName(fighterInfo:CharacterMinimalPlusLookInformations) : String
      {
         var breed:Breed = null;
         var breedText:String = null;
         var fullName:String = null;
         if(fighterInfo.breed > 0)
         {
            breed = this.dataApi.getBreed(fighterInfo.breed);
            breedText = breed.name + " ";
         }
         else
         {
            breedText = "";
         }
         var name:String = fighterInfo.name;
         if(name == "3451")
         {
            name = this._prismText;
         }
         if(fighterInfo.level > ProtocolConstantsEnum.MAX_LEVEL)
         {
            fullName = name + "  (" + breedText + this._prestigeText + (fighterInfo.level - ProtocolConstantsEnum.MAX_LEVEL) + ")";
         }
         else
         {
            fullName = name + "  (" + breedText + this._levelText + fighterInfo.level + ")";
         }
         return fullName;
      }
      
      private function onTaxCollectorsListUpdate(infoType:uint = 0) : void
      {
         var fight:SocialEntityInFightWrapper = null;
         var newList:Array = new Array();
         for each(fight in this._fightsList)
         {
            if(fight.typeId != TYPE_TAX_COLLECTOR)
            {
               newList.push(fight);
            }
         }
         this._fightsList = newList;
         for each(fight in this.socialApi.getAllFightingTaxCollectors())
         {
            this._fightsList.push(fight);
            if(!this._fightsInfoForDisplay[fight.uniqueId])
            {
               this.prepareOneFightData(fight);
            }
         }
         this.refreshGrid();
      }
      
      private function onTaxCollectorUpdate(id:Number) : void
      {
         var fight:SocialEntityInFightWrapper = null;
         var tc:SocialEntityInFightWrapper = this.socialApi.getAllFightingTaxCollector(id);
         var index:int = -1;
         for each(fight in this._fightsList)
         {
            if(fight.uniqueId == id && fight.typeId == TYPE_TAX_COLLECTOR)
            {
               index = this._fightsList.indexOf(fight);
            }
         }
         if(index > -1 && !tc)
         {
            this._fightsList.splice(index,1);
            this.removeOneFightData(id);
            this.refreshGrid();
         }
         else if(index == -1 && Boolean(tc))
         {
            this.prepareOneFightData(tc);
            this._fightsList.push(tc);
            this.refreshGrid();
         }
      }
      
      private function onAllianceTaxCollectorRemoved(uniqueId:Number) : void
      {
         var fight:SocialEntityInFightWrapper = null;
         var index:int = -1;
         for each(fight in this._fightsList)
         {
            if(fight.uniqueId == uniqueId && fight.typeId == TYPE_TAX_COLLECTOR)
            {
               index = this._fightsList.indexOf(fight);
            }
         }
         if(index > -1)
         {
            this._fightsList.splice(index,1);
            this.removeOneFightData(uniqueId);
            this.refreshGrid();
         }
      }
      
      private function onPrismsInFightList() : void
      {
         var fight:SocialEntityInFightWrapper = null;
         var newList:Array = new Array();
         for each(fight in this._fightsList)
         {
            if(fight.typeId != TYPE_PRISM)
            {
               newList.push(fight);
            }
         }
         this._fightsList = newList;
         for each(fight in this.socialApi.getFightingPrisms())
         {
            this._fightsList.push(fight);
            this.prepareOneFightData(fight);
         }
         this.refreshGrid();
      }
      
      private function onPrismInFightAdded(subAreaId:int) : void
      {
         var p:SocialEntityInFightWrapper = this.socialApi.getFightingPrism(subAreaId);
         this._fightsList.push(p);
         this.prepareOneFightData(p);
         this.refreshGrid();
      }
      
      private function onPrismInFightRemoved(subAreaId:int) : void
      {
         var fight:SocialEntityInFightWrapper = null;
         var index:int = -1;
         for each(fight in this._fightsList)
         {
            if(fight.uniqueId == subAreaId && fight.typeId == TYPE_PRISM)
            {
               index = this._fightsList.indexOf(fight);
            }
         }
         if(index > -1)
         {
            this._fightsList.splice(index,1);
            this.removeOneFightData(subAreaId);
            this.refreshGrid();
         }
      }
      
      private function onGuildFightEnnemiesListUpdate(type:int, fightId:Number) : void
      {
         var enemies:Array = null;
         var e2:* = undefined;
         if(type == 0)
         {
            enemies = this.socialApi.getAllFightingTaxCollector(fightId).enemyCharactersInformations;
         }
         else if(type == 1)
         {
            enemies = this.socialApi.getFightingPrism(fightId).enemyCharactersInformations;
         }
         var enemiesNames:String = this.uiApi.getText("ui.common.attackers") + " : \n";
         if(Boolean(enemies) && enemies.length > 0)
         {
            for each(e2 in enemies)
            {
               enemiesNames += this.getFighterName(e2.playerCharactersInformations) + "\n";
            }
         }
         this._attackers[fightId] = {
            "charactersInfoList":enemies,
            "charactersNameList":enemiesNames
         };
         this.refreshOneFight(fightId);
      }
      
      private function onGuildFightAlliesListUpdate(type:int, fightId:Number) : void
      {
         var allies:Array = null;
         var a2:* = undefined;
         if(type == 0)
         {
            allies = this.socialApi.getAllFightingTaxCollector(fightId).allyCharactersInformations;
         }
         else if(type == 1)
         {
            allies = this.socialApi.getFightingPrism(fightId).allyCharactersInformations;
         }
         var alliesNames:String = this.uiApi.getText("ui.common.defenders") + " : \n";
         if(Boolean(allies) && allies.length > 0)
         {
            for each(a2 in allies)
            {
               alliesNames += this.getFighterName(a2.playerCharactersInformations) + "\n";
            }
         }
         this._defenders[fightId] = {
            "charactersInfoList":allies,
            "charactersNameList":alliesNames
         };
         this.refreshOneFight(fightId);
      }
      
      public function onRollOver(target:Object) : void
      {
         var text:String = null;
         var data:Object = null;
         var fightersInfo:Object = null;
         if(target.name.indexOf("tx_attackTeam") != -1)
         {
            data = this._interactiveComponentsList[target.name];
            fightersInfo = this._attackers[data.uniqueId];
            if(Boolean(fightersInfo) && fightersInfo.charactersNameList != "")
            {
               text = fightersInfo.charactersNameList;
            }
         }
         else if(target.name.indexOf("tx_defenseTeam") != -1)
         {
            data = this._interactiveComponentsList[target.name];
            fightersInfo = this._defenders[data.uniqueId];
            if(Boolean(fightersInfo) && fightersInfo.charactersNameList != "")
            {
               text = fightersInfo.charactersNameList;
            }
         }
         else if(target.name.indexOf("tx_spectate") != -1)
         {
            text = this.uiApi.getText("ui.spectator.clicToJoin");
         }
         if(text)
         {
            this.uiApi.showTooltip(this.uiApi.textTooltipInfo(text),target,false,"standard",7,1,3,null,null,null,"TextInfo");
         }
      }
      
      public function onRollOut(target:Object) : void
      {
         this.uiApi.hideTooltip();
      }
      
      public function onRelease(target:Object) : void
      {
         var data:Object = null;
         var attackers:Object = null;
         var firstAttacker:SocialFightersWrapper = null;
         if(target.name.indexOf("tx_spectate") != -1)
         {
            data = this._interactiveComponentsList[target.name];
            attackers = this._attackers[data.uniqueId];
            if(Boolean(attackers) && Boolean(attackers.charactersInfoList) && attackers.charactersInfoList.length > 0)
            {
               firstAttacker = attackers.charactersInfoList[0];
               this.sysApi.sendAction(new GameFightSpectatePlayerRequest(firstAttacker.playerCharactersInformations.id));
            }
         }
      }
      
      public function onSelectItem(target:Object, selectMethod:uint, isNewSelection:Boolean) : void
      {
         var data:Object = null;
         if(target.name.indexOf("gd_defenseTeam") != -1 && selectMethod == SelectMethodEnum.CLICK)
         {
            data = this._interactiveComponentsList[target.name];
            if(target.selectedItem.hasOwnProperty("playerCharactersInformations"))
            {
               if(target.selectedItem.playerCharactersInformations.id != this.playerApi.id())
               {
                  JoinFightUtil.swapPlaces(data.typeId,data.uniqueId,target.selectedItem.playerCharactersInformations);
               }
               else
               {
                  JoinFightUtil.leave(data.typeId,data.uniqueId);
               }
            }
         }
      }
      
      public function onSelectEmptyItem(target:Object, selectMethod:uint) : void
      {
         var data:Object = null;
         if(target.name.indexOf("gd_defenseTeam") != -1 && selectMethod == SelectMethodEnum.CLICK)
         {
            data = this._interactiveComponentsList[target.name];
            JoinFightUtil.join(data.typeId,data.uniqueId);
         }
      }
      
      public function onItemRollOver(target:Object, item:Object) : void
      {
         var text:String = null;
         var fighter:Object = null;
         if(Boolean(item) && Boolean(item.data))
         {
            text = "";
            if(target.name.indexOf("gd_attackTeam") != -1)
            {
               fighter = item.data.playerCharactersInformations;
               text = this.getFighterName(fighter as CharacterMinimalPlusLookInformations);
               if(fighter is CharacterMinimalAllianceInformations)
               {
                  text += " - " + fighter.guild.guildName + " [" + fighter.alliance.allianceTag + "]";
               }
               else if(fighter is CharacterMinimalGuildInformations)
               {
                  text += " - " + fighter.guild.guildName;
               }
            }
            if(target.name.indexOf("gd_defenseTeam") != -1)
            {
               fighter = item.data.playerCharactersInformations;
               text = this.getFighterName(fighter as CharacterMinimalPlusLookInformations);
               if(fighter is CharacterMinimalGuildInformations || fighter is CharacterMinimalAllianceInformations)
               {
                  text += " - " + fighter.guild.guildName;
               }
            }
            if(text != "")
            {
               this.uiApi.showTooltip(this.uiApi.textTooltipInfo(text),item.container,false,"standard",this.sysApi.getEnum("com.ankamagames.berilia.types.LocationEnum").POINT_BOTTOMRIGHT,1,3,null,null,null,"TextInfo");
            }
         }
      }
      
      public function onItemRollOut(target:Object, item:Object) : void
      {
         this.uiApi.hideTooltip();
      }
      
      public function onEnterFrame() : void
      {
         var percentTime:Number = NaN;
         var fightWithTimer:Object = null;
         var fightId:Number = NaN;
         var clock:uint = uint(getTimer());
         var fightsToRemoveIds:Array = new Array();
         for each(fightWithTimer in this._fightsTimerInfos)
         {
            if(fightWithTimer.clockDuration == 0 || fightWithTimer.clockEnd == 0)
            {
               percentTime = 1;
            }
            else
            {
               percentTime = (fightWithTimer.clockDuration - (fightWithTimer.clockEnd - clock)) / fightWithTimer.clockDuration;
               percentTime = Math.round(percentTime * 100) / 100;
            }
            if(this._fightsInfoForDisplay[fightWithTimer.uniqueId].elapsedTimePercent != percentTime)
            {
               if(clock >= fightWithTimer.clockEnd)
               {
                  this._fightsInfoForDisplay[fightWithTimer.uniqueId].startFight();
                  fightsToRemoveIds.push(fightWithTimer.uniqueId);
               }
               this._fightsInfoForDisplay[fightWithTimer.uniqueId].elapsedTimePercent = percentTime;
               this.refreshOneFight(fightWithTimer.uniqueId);
            }
         }
         for each(fightId in fightsToRemoveIds)
         {
            this.removeFightTimerInfo(fightId);
         }
      }
   }
}

