package ui
{
   import com.ankamagames.berilia.api.UiApi;
   import com.ankamagames.berilia.components.Label;
   import com.ankamagames.berilia.components.Texture;
   import com.ankamagames.berilia.components.TextureBitmap;
   import com.ankamagames.berilia.types.graphic.GraphicContainer;
   import com.ankamagames.dofus.datacenter.idols.Idol;
   import com.ankamagames.dofus.datacenter.monsters.Monster;
   import com.ankamagames.dofus.internalDatacenter.conquest.PrismSubAreaWrapper;
   import com.ankamagames.dofus.internalDatacenter.people.PartyMemberWrapper;
   import com.ankamagames.dofus.network.types.game.context.EntityDispositionInformations;
   import com.ankamagames.dofus.network.types.game.context.roleplay.GameRolePlayGroupMonsterWaveInformations;
   import com.ankamagames.dofus.network.types.game.context.roleplay.GroupMonsterStaticInformations;
   import com.ankamagames.dofus.network.types.game.context.roleplay.GroupMonsterStaticInformationsWithAlternatives;
   import com.ankamagames.dofus.network.types.game.context.roleplay.MonsterInGroupLightInformations;
   import com.ankamagames.dofus.network.types.game.guild.GuildMember;
   import com.ankamagames.dofus.uiApi.DataApi;
   import com.ankamagames.dofus.uiApi.MapApi;
   import com.ankamagames.dofus.uiApi.PartyApi;
   import com.ankamagames.dofus.uiApi.PlayedCharacterApi;
   import com.ankamagames.dofus.uiApi.RoleplayApi;
   import com.ankamagames.dofus.uiApi.QuestApi;
   import flash.text.TextField;
   import flash.text.TextLineMetrics;
   import flash.geom.Rectangle;
   import com.ankamagames.dofus.uiApi.SocialApi;
   import com.ankamagames.dofus.uiApi.SystemApi;
   import com.ankamagames.dofus.uiApi.TimeApi;
   import com.ankamagames.dofus.uiApi.TooltipApi;
   import com.ankamagames.dofus.uiApi.UtilApi;
   import com.ankamagames.dofusModuleLibrary.enum.AlignementSideEnum;
   import d2enums.ProtocolConstantsEnum;
   import d2hooks.UiLoaded;
   import flash.geom.ColorTransform;
   import utils.FormulaHandler;
   
   public class WorldRpMonstersGroupTooltipUi
   {
      
      private static const WAVE_COLOR_TRANSFORM:ColorTransform = new ColorTransform(1,1,1,1,202,173,-13);
      
      public var tooltipApi:TooltipApi;
      
      public var uiApi:UiApi;
      
      public var utilApi:UtilApi;
      
      public var dataApi:DataApi;
      
      public var mapApi:MapApi;
      
      public var partyApi:PartyApi;
      
      public var playerApi:PlayedCharacterApi;
      
      public var sysApi:SystemApi;
      
      public var socialApi:SocialApi;
      
      public var timeApi:TimeApi;
      
      public var roleplayApi:RoleplayApi;

      public var questApi:QuestApi;

      private static var s_succesParMonstre:Object = null;
      
      private var _monstersInfo:GroupMonsterStaticInformations;
      
      private var _useScale:Boolean = false;
      
      private var _disposition:EntityDispositionInformations;
      
      public var mainCtr:GraphicContainer;
      
      public var ctr_wave:GraphicContainer;
      
      public var ctr_xp:GraphicContainer;
      
      public var ctr_malusDropArena:GraphicContainer;
      
      public var ctr_back:GraphicContainer;
      
      public var ctr_main:GraphicContainer;
      
      public var ctr_groupOptimal:GraphicContainer;
      
      public var ctr_malusDrop:GraphicContainer;
      
      public var ctr_header:GraphicContainer;
      
      public var ctr_separatorMalus:GraphicContainer;
      
      public var ctr_separatorXp:GraphicContainer;
      
      public var lbl_level:Label;
      
      public var lbl_monsterList:Label;
      
      public var lbl_disabledMonsterList:Label;
      
      public var lbl_nbWaves:Label;
      
      public var lbl_monsterXp:Label;
      
      public var lbl_groupOptimal:Label;
      
      public var lbl_malusDrop:Label;
      
      public var tx_wave:Texture;
      
      public var tx_sword:TextureBitmap;
      
      public function WorldRpMonstersGroupTooltipUi()
      {
         super();
      }
      
      public function main(oParam:Object = null) : void
      {
         var monster:Monster = null;
         var member:PartyMemberWrapper = null;
         var wisdom:int = 0;
         var pg:GuildMember = null;
         var xpAlliancePrismBonus:Number = NaN;
         var m:Object = null;
         var nbCompanions:int = 0;
         var i:int = 0;
         var prismInfo:PrismSubAreaWrapper = null;
         var waves:Object = null;
         var waveInfo:GroupMonsterStaticInformations = null;
         var waveXpSolo:Number = NaN;
         var waveXpGroup:Number = NaN;
         var waveData:MonstersGroupData = null;
         var malus:int = 0;
         this.sysApi.addHook(UiLoaded,this.onUiLoaded);
         var data:Object = oParam.data;
         if(data.hasOwnProperty("useScale"))
         {
            this._useScale = data.useScale;
            this._monstersInfo = data.monsterGroupInfos.staticInfos;
            this._disposition = data.monsterGroupInfos.disposition;
         }
         else
         {
            this._monstersInfo = data.staticInfos;
            this._disposition = data.disposition;
         }
         this.ctr_back.width = 1;
         this.ctr_wave.height = 1;
         this.lbl_monsterList.width = 1;
         this.lbl_monsterXp.width = 1;
         this.lbl_disabledMonsterList.width = 1;
         this.ctr_header.width = 1;
         this.ctr_groupOptimal.width = 1;
         this.ctr_malusDrop.width = 1;
         this.ctr_separatorMalus.width = 1;
         this.ctr_separatorXp.width = 1;
         this.ctr_malusDropArena.height = 1;
         if(data.alignmentSide == AlignementSideEnum.ALIGNMENT_ANGEL)
         {
            this.lbl_monsterList.cssClass = "bonta";
            this.lbl_disabledMonsterList.cssClass = "bonta";
         }
         else if(data.alignmentSide == AlignementSideEnum.ALIGNMENT_EVIL)
         {
            this.lbl_monsterList.cssClass = "brakmar";
            this.lbl_disabledMonsterList.cssClass = "brakmar";
         }
         else
         {
            this.lbl_monsterList.cssClass = "center";
            this.lbl_disabledMonsterList.cssClass = "center";
         }
         var partyMembers:Object = this.partyApi.getPartyMembers();
         var currentPlayerLevel:uint = Math.min(this.playerApi.getPlayedCharacterInfo().level,ProtocolConstantsEnum.MAX_LEVEL);
         var currentPlayerCharac:Object = this.playerApi.characteristics();
         var formulaGroupMembers:Array = [];
         if(partyMembers.length == 0 && this.playerApi.hasCompanion())
         {
            formulaGroupMembers.push(FormulaHandler.createGroupMember(currentPlayerLevel));
            formulaGroupMembers.push(FormulaHandler.createGroupMember(currentPlayerLevel,true));
         }
         else
         {
            for each(member in partyMembers)
            {
               formulaGroupMembers.push(FormulaHandler.createGroupMember(Math.min(member.level,ProtocolConstantsEnum.MAX_LEVEL)));
               nbCompanions = int(member.companions.length);
               for(i = 0; i < nbCompanions; i++)
               {
                  formulaGroupMembers.push(FormulaHandler.createGroupMember(Math.min(member.level,ProtocolConstantsEnum.MAX_LEVEL),true));
               }
            }
         }
         wisdom = currentPlayerCharac.wisdom.base + currentPlayerCharac.wisdom.additionnal + currentPlayerCharac.wisdom.objectsAndMountBonus + currentPlayerCharac.wisdom.alignGiftBonus + currentPlayerCharac.wisdom.contextModif;
         pg = this.getPlayerGuild(this.playerApi.getPlayedCharacterInfo().id);
         if(this.socialApi.hasAlliance())
         {
            prismInfo = this.socialApi.getPrismSubAreaById(this.playerApi.currentSubArea().id);
            if(Boolean(this.socialApi.hasAlliance() && prismInfo) && Boolean(prismInfo.mapId != -1) && (Boolean(!prismInfo.alliance) || Boolean(prismInfo.alliance && prismInfo.alliance.allianceId == this.socialApi.getAlliance().allianceId)))
            {
               xpAlliancePrismBonus = 25;
            }
         }
         var playerData:* = FormulaHandler.createPlayerData(currentPlayerLevel,wisdom,this.playerApi.getExperienceBonusPercent(),this.playerApi.getMount() != null && this.playerApi.isRidding() ? Number(this.playerApi.getMount().xpRatio) : 0,pg != null ? pg.experienceGivenPercent : 0,xpAlliancePrismBonus);
         var groupData:MonstersGroupData = this.getMonstersGroupData(this._monstersInfo,playerData,formulaGroupMembers,this.mapApi.getMapTotalRewardRate());
         this.lbl_level.text = this.uiApi.getText("ui.common.level") + " " + groupData.level;
         var textList:String = "";
         var textListDisabled:String = "";
         var grp:Array = groupData.group;
         var lignes:Array = [];
         var avecTrophees:Boolean = false;
         for each(m in grp)
         {
            if(m.visible)
            {
               monster = this.dataApi.getMonsterFromId(m.monsterId);
               lignes.push(m.monsterId);
               textList += "\n" + (monster.isMiniBoss ? "<b>" : "") + monster.name + " (" + m.level + ")" + (monster.isMiniBoss ? "</b>" : "");
            }
            else
            {
               monster = this.dataApi.getMonsterFromId(m.monsterId);
               textListDisabled += "\n" + monster.name + " (" + m.level + ")";
            }
         }
         if(textList)
         {
            textList = textList.substr(1);
         }
         var xpText:String = "";
         var inParty:Boolean = formulaGroupMembers.length > 0;
         var grpgmwi:GameRolePlayGroupMonsterWaveInformations = data as GameRolePlayGroupMonsterWaveInformations;
         if(grpgmwi)
         {
            this.tx_wave.colorTransform = WAVE_COLOR_TRANSFORM;
            this.ctr_wave.visible = true;
            this.lbl_nbWaves.text = "x " + (grpgmwi.nbWaves + 1);
            this.lbl_nbWaves.fullWidthAndHeight();
            this.ctr_wave.height = this.lbl_nbWaves.height;
            waves = grpgmwi.alternatives;
            waveXpSolo = groupData.xpSolo;
            waveXpGroup = groupData.xpGroup;
            for each(waveInfo in waves)
            {
               waveData = this.getMonstersGroupData(waveInfo,playerData,formulaGroupMembers,this.mapApi.getMapTotalRewardRate());
               waveXpSolo += waveData.xpSolo;
               waveXpGroup += waveData.xpGroup;
            }
            xpText += this.uiApi.getText("ui.tooltip.monsterXpAlone",this.utilApi.formateIntToString(waveXpSolo));
            if(inParty)
            {
               xpText += "\n" + this.uiApi.getText("ui.tooltip.monsterXpParty",this.utilApi.formateIntToString(waveXpGroup));
            }
         }
         else
         {
            this.hideBloc(this.ctr_wave);
            xpText += this.uiApi.getText("ui.tooltip.monsterXpAlone",this.utilApi.formateIntToString(groupData.xpSolo));
            if(inParty)
            {
               xpText += "\n" + this.uiApi.getText("ui.tooltip.monsterXpParty",this.utilApi.formateIntToString(groupData.xpGroup));
            }
         }
         this.lbl_monsterXp.text = xpText;
         this.lbl_monsterList.text = textList;
         this.lbl_disabledMonsterList.visible = textListDisabled != "";
         this.lbl_disabledMonsterList.text = textListDisabled;
         this.lbl_monsterList.fullWidthAndHeight();
         this.lbl_disabledMonsterList.fullWidthAndHeight();
         this.lbl_monsterXp.fullWidthAndHeight();
         this.lbl_level.fullWidthAndHeight();
         if(data.lootShare != null && data.lootShare > 0)
         {
            malus = FormulaHandler.getArenaMalusDrop(data.lootShare,partyMembers.length);
            if(malus > 0)
            {
               this.ctr_separatorMalus.visible = true;
               this.lbl_groupOptimal.text = this.uiApi.getText("ui.tooltip.maxMemberParty",data.lootShare);
               this.lbl_groupOptimal.fullWidthAndHeight();
               this.lbl_malusDrop.text = this.uiApi.getText("ui.tooltip.arenaMalus",malus);
               this.lbl_malusDrop.fullWidthAndHeight();
               this.ctr_malusDropArena.visible = true;
            }
            else
            {
               this.ctr_separatorMalus.visible = false;
               this.hideBloc(this.ctr_malusDropArena);
            }
         }
         else
         {
            this.ctr_separatorMalus.visible = false;
            this.hideBloc(this.ctr_malusDropArena);
         }
         var maxWidth:int = this.getMaxWidth();
         avecTrophees = this.compterTrophees(lignes) > 0;
         if(avecTrophees)
         {
            maxWidth += 36;
         }
         this.lbl_monsterList.width = maxWidth;
         this.lbl_monsterXp.width = maxWidth;
         this.lbl_disabledMonsterList.width = maxWidth;
         this.ctr_xp.width = maxWidth;
         this.ctr_header.width = maxWidth;
         this.ctr_main.width = maxWidth;
         this.ctr_groupOptimal.width = maxWidth;
         this.ctr_malusDrop.width = maxWidth;
         this.ctr_malusDropArena.width = maxWidth;
         this.ctr_separatorMalus.width = maxWidth;
         this.ctr_separatorXp.width = maxWidth;
         if(this.ctr_wave.visible)
         {
            this.tx_wave.x = maxWidth / 2 - (this.tx_wave.width + this.lbl_nbWaves.width) / 2;
            this.lbl_nbWaves.x = this.tx_wave.x + this.tx_wave.width;
         }
         this.ctr_back.width = maxWidth + 8;
         if(this.ctr_malusDropArena.visible)
         {
            this.ctr_malusDropArena.height = this.lbl_groupOptimal.textHeight + this.lbl_malusDrop.textHeight;
         }
         var uiLoading:Boolean = this.uiApi.isUiLoading(this.uiApi.me().name);
         this.uiApi.me().render();
         if(!uiLoading)
         {
            this.onUiLoaded(this.uiApi.me().name);
         }
         this.placerTrophees(avecTrophees ? lignes : null);
         var firstInit:Boolean = this.ctr_back.height == 0;
         var backHeight:Number = this.ctr_main.height;
         var th:int = this.ctr_separatorXp.y + this.lbl_monsterList.height + this.lbl_disabledMonsterList.height - (this.lbl_disabledMonsterList.visible ? 25 : 0);
         if(backHeight < th)
         {
            backHeight = th;
         }
         if(firstInit)
         {
            if(inParty)
            {
               backHeight += 21;
            }
            if(this.ctr_wave.visible)
            {
               backHeight += 13;
            }
            else
            {
               backHeight -= 20;
            }
         }
         if(!firstInit && this.lbl_disabledMonsterList.visible)
         {
            backHeight += 8;
         }
         this.ctr_back.height = backHeight;
         this.tooltipApi.place(oParam.position,oParam.showDirectionalArrow,oParam.point,oParam.relativePoint,oParam.offset,true,this._disposition.cellId);
      }
      
      private function sansSucces(monsterId:int) : Boolean
      {
         var o:Object = null;
         var re:RegExp = null;
         var r:Object = null;
         var a:Array = null;
         var finis:Array = null;
         var id:int = 0;
         if(!this.questApi)
         {
            return false;
         }
         if(!s_succesParMonstre)
         {
            s_succesParMonstre = new Object();
            for each(o in this.dataApi.getAchievementObjectives())
            {
               if(o && o.criterion)
               {
                  re = /(Ef|EM)>(\d+)/g;
                  r = re.exec(o.criterion);
                  while(r != null)
                  {
                     a = s_succesParMonstre[r[2]] as Array;
                     if(!a)
                     {
                        a = [];
                        s_succesParMonstre[r[2]] = a;
                     }
                     if(a.indexOf(o.achievementId) == -1)
                     {
                        a.push(o.achievementId);
                     }
                     r = re.exec(o.criterion);
                  }
               }
            }
         }
         a = s_succesParMonstre[String(monsterId)] as Array;
         if(!a)
         {
            return false;
         }
         finis = this.questApi.getFinishedCharacterAchievementIds().concat(this.questApi.getFinishedAccountAchievementIds());
         for each(id in a)
         {
            if(finis.indexOf(id) == -1)
            {
               return true;
            }
         }
         return false;
      }

      private function compterTrophees(ids:Array) : int
      {
         var n:int = 0;
         var id:int = 0;
         try
         {
            for each(id in ids)
            {
               if(this.sansSucces(id))
               {
                  n++;
               }
            }
         }
         catch(e:Error)
         {
            return 0;
         }
         return n;
      }

      private function placerTrophees(ids:Array) : void
      {
         var i:int = 0;
         var tx:Texture = null;
         var tf:TextField = null;
         var lm:TextLineMetrics = null;
         var r:Rectangle = null;
         var y:Number = 0;
         i = this.lbl_monsterList.numChildren - 1;
         while(i >= 0)
         {
            if(this.lbl_monsterList.getChildAt(i).name.indexOf("trophee") == 0)
            {
               this.lbl_monsterList.removeChildAt(i);
            }
            i--;
         }
         if(!ids || ids.length == 0)
         {
            return;
         }
         try
         {
            tf = this.lbl_monsterList.textfield;
            if(!tf)
            {
               return;
            }
            i = 0;
            while(i < ids.length && i < tf.numLines)
            {
               if(this.sansSucces(ids[i]))
               {
                  lm = tf.getLineMetrics(i);
                  // PADGREF 21/10 : position prise sur le DERNIER caractere visible de la ligne (la 1re ligne avait des
                  // metriques fausses : trophee pose sur la parenthese)
                  var debut:int = tf.getLineOffset(i);
                  var fin:int = debut + tf.getLineLength(i) - 1;
                  while(fin > debut && " \r\n\t".indexOf(tf.text.charAt(fin)) >= 0)
                  {
                     fin--;
                  }
                  r = tf.getCharBoundaries(fin);
                  y = r ? r.y : i * lm.height;
                  tx = this.uiApi.createComponent("Texture") as Texture;
                  tx.name = "trophee" + i;
                  tx.width = 14;
                  tx.height = 14;
                  tx.x = tf.x + (r ? r.x + r.width : lm.x + lm.width) + 4;
                  tx.y = tf.y + y + (lm.height - 14) / 2;
                  tx.uri = this.uiApi.createUri(this.sysApi.getConfigEntry("config.ui.skin") + "texture/icon_trophy.png");
                  tx.finalize();
                  this.lbl_monsterList.addChild(tx);
               }
               i++;
            }
         }
         catch(e:Error)
         {
         }
      }

      private function getMaxWidth() : int
      {
         var maxValue:int = 0;
         if(this.lbl_level.width > this.lbl_monsterList.width && this.lbl_level.width > this.lbl_monsterXp.width && this.lbl_level.width > this.lbl_disabledMonsterList.width)
         {
            maxValue = this.lbl_level.width;
         }
         else if(this.lbl_groupOptimal.visible && this.lbl_groupOptimal.width > this.lbl_monsterList.width && this.lbl_groupOptimal.width > this.lbl_monsterXp.width && this.lbl_groupOptimal.width > this.lbl_disabledMonsterList.width && this.lbl_groupOptimal.width > this.lbl_level.width)
         {
            maxValue = this.lbl_groupOptimal.width;
         }
         else if(this.lbl_malusDrop.visible && this.lbl_malusDrop.width > this.lbl_monsterList.width && this.lbl_malusDrop.width > this.lbl_monsterXp.width && this.lbl_malusDrop.width > this.lbl_disabledMonsterList.width && this.lbl_malusDrop.width > this.lbl_level.width)
         {
            maxValue = this.lbl_malusDrop.width;
         }
         else if(this.lbl_disabledMonsterList.visible && this.lbl_disabledMonsterList.width > this.lbl_monsterXp.width && this.lbl_disabledMonsterList.width >= this.lbl_disabledMonsterList.width && this.lbl_disabledMonsterList.width > this.lbl_monsterList.width && this.lbl_disabledMonsterList.width > this.lbl_level.width)
         {
            maxValue = this.lbl_disabledMonsterList.width;
         }
         else if(this.lbl_monsterXp.visible && this.lbl_monsterXp.width > this.lbl_monsterList.width && this.lbl_monsterXp.width > this.lbl_disabledMonsterList.width && this.lbl_monsterXp.width > this.lbl_level.width)
         {
            maxValue = this.lbl_monsterXp.width;
         }
         else
         {
            maxValue = this.lbl_monsterList.width;
         }
         return maxValue;
      }
      
      private function onUiLoaded(uiName:String) : void
      {
         var diff:Number = NaN;
         var offset:Number = NaN;
         if(uiName == this.uiApi.me().name)
         {
            if(this.roleplayApi.isMonstersGroupAttacking(this._monstersInfo))
            {
               this.tx_sword.x = this.lbl_level.x + this.lbl_level.width + 5;
               this.tx_sword.visible = true;
               diff = this.ctr_back.width - (this.tx_sword.x + this.tx_sword.width);
               if(diff < 8)
               {
                  offset = 8 - diff;
                  this.ctr_back.width += offset + 4;
               }
            }
            else
            {
               this.tx_sword.visible = false;
            }
         }
      }
      
      private function hideBloc(ctr:GraphicContainer) : void
      {
         ctr.height = 0;
         ctr.visible = false;
      }
      
      public function getRealMonsterGrade(realGroup:Array, creatureGenericId:int) : int
      {
         var mob:MonsterInGroupLightInformations = null;
         if(realGroup == null)
         {
            return 0;
         }
         for each(mob in realGroup)
         {
            if(mob.genericId == creatureGenericId)
            {
               realGroup.splice(realGroup.indexOf(mob),1);
               return mob.grade;
            }
         }
         return -1;
      }
      
      private function xtremAdvancedSortMonster(monsterA:Object, monsterB:Object) : Number
      {
         var result:int = 0;
         if(monsterA.monsterId == monsterB.monsterId)
         {
            if(monsterA.level > monsterB.level)
            {
               result = -1;
            }
            else if(monsterA.level < monsterB.level)
            {
               result = 1;
            }
            else
            {
               result = 0;
            }
         }
         else if(monsterA.maxLevel > monsterB.maxLevel)
         {
            result = -1;
         }
         else if(monsterA.maxLevel < monsterB.maxLevel)
         {
            result = 1;
         }
         else if(monsterA.totalLevel > monsterB.totalLevel)
         {
            result = -1;
         }
         else if(monsterA.totalLevel < monsterB.totalLevel)
         {
            result = 1;
         }
         else
         {
            result = 0;
         }
         return result;
      }
      
      private function truncate(val:Number) : int
      {
         var multiplier:uint = Math.pow(10,0);
         var truncatedVal:Number = val * multiplier;
         return int(truncatedVal) / multiplier;
      }
      
      private function getPlayerGuild(idPlayer:Number) : GuildMember
      {
         var mem:GuildMember = null;
         for each(mem in this.socialApi.getGuildMembers())
         {
            if(mem.id == idPlayer)
            {
               return mem;
            }
         }
         return null;
      }
      
      private function getMonstersGroupData(pStaticInfos:GroupMonsterStaticInformations, pPlayerData:*, pGroupMembersData:Array, pMapRewardRate:int) : MonstersGroupData
      {
         var groupData:MonstersGroupData = null;
         var mainCreatureLvl:int = 0;
         var monsterData:* = undefined;
         var member:PartyMemberWrapper = null;
         var monster:Monster = null;
         var lvlMonster:int = 0;
         var realMonstersList:Array = null;
         var totalLevel:uint = 0;
         var formulaMobs:Array = null;
         var m:Object = null;
         var numFirstMonsters:uint = 0;
         var idolsExpBonusTotalPercentSolo:uint = 0;
         var idolsExpBonusTotalPercentGroup:uint = 0;
         var members:int = 0;
         var realMonstersListObject:Object = null;
         var realGroup:Object = null;
         var val:Object = null;
         var monstersLevel:Array = new Array();
         var maxMonsterLevel:Array = new Array();
         mainCreatureLvl = int(pStaticInfos.mainCreatureLightInfos.level);
         monstersLevel[pStaticInfos.mainCreatureLightInfos.genericId] = mainCreatureLvl;
         maxMonsterLevel[pStaticInfos.mainCreatureLightInfos.genericId] = mainCreatureLvl;
         var partyMembers:Object = this.partyApi.getPartyMembers();
         for each(monsterData in pStaticInfos.underlings)
         {
            monster = this.dataApi.getMonsterFromId(monsterData.genericId);
            lvlMonster = int(monsterData.level);
            if(Boolean(monstersLevel[monsterData.genericId]) && monstersLevel[monsterData.genericId] > 0)
            {
               monstersLevel[monsterData.genericId] += lvlMonster;
            }
            else
            {
               monstersLevel[monsterData.genericId] = lvlMonster;
            }
            if(Boolean(maxMonsterLevel[monsterData.genericId]) && maxMonsterLevel[monsterData.genericId] > 0)
            {
               if(maxMonsterLevel[monsterData.genericId] < lvlMonster)
               {
                  maxMonsterLevel[monsterData.genericId] = lvlMonster;
               }
            }
            else
            {
               maxMonsterLevel[monsterData.genericId] = lvlMonster;
            }
         }
         if(pStaticInfos is GroupMonsterStaticInformationsWithAlternatives)
         {
            realMonstersList = new Array();
            members = int(partyMembers.length);
            if(members == 0 && this.playerApi.hasCompanion())
            {
               members = 2;
            }
            else
            {
               for each(member in partyMembers)
               {
                  members += member.companions.length;
               }
            }
            for each(realGroup in (pStaticInfos as GroupMonsterStaticInformationsWithAlternatives).alternatives)
            {
               if(realMonstersListObject == null || realGroup.playerCount <= members)
               {
                  realMonstersListObject = realGroup;
               }
            }
            for each(val in realMonstersListObject.monsters)
            {
               realMonstersList.push(val);
            }
         }
         var grp:Array = [];
         var m1:Monster = this.dataApi.getMonsterFromId(pStaticInfos.mainCreatureLightInfos.genericId);
         var realMonsterGrade:int = this.getRealMonsterGrade(realMonstersList,pStaticInfos.mainCreatureLightInfos.genericId);
         totalLevel = pStaticInfos.mainCreatureLightInfos.level;
         grp.push({
            "monsterId":pStaticInfos.mainCreatureLightInfos.genericId,
            "level":totalLevel,
            "hiddenLevel":m1.grades[pStaticInfos.mainCreatureLightInfos.grade - 1].hiddenLevel,
            "gradeXp":this.getXp(pStaticInfos.mainCreatureLightInfos,m1,realMonsterGrade),
            "totalLevel":monstersLevel[pStaticInfos.mainCreatureLightInfos.genericId],
            "maxLevel":maxMonsterLevel[pStaticInfos.mainCreatureLightInfos.genericId],
            "visible":realMonsterGrade >= 0
         });
         for each(monsterData in pStaticInfos.underlings)
         {
            m1 = this.dataApi.getMonsterFromId(monsterData.genericId);
            realMonsterGrade = this.getRealMonsterGrade(realMonstersList,monsterData.genericId);
            lvlMonster = int(monsterData.level);
            if(realMonsterGrade >= 0)
            {
               totalLevel += lvlMonster;
            }
            grp.push({
               "monsterId":monsterData.genericId,
               "level":lvlMonster,
               "hiddenLevel":m1.grades[monsterData.grade - 1].hiddenLevel,
               "gradeXp":this.getXp(monsterData,m1,realMonsterGrade),
               "totalLevel":monstersLevel[monsterData.genericId],
               "maxLevel":maxMonsterLevel[monsterData.genericId],
               "visible":realMonsterGrade >= 0
            });
         }
         grp.sort(this.xtremAdvancedSortMonster);
         formulaMobs = new Array();
         for each(m in grp)
         {
            if(m.visible)
            {
               formulaMobs.push(FormulaHandler.createMonsterData(m.level,m.gradeXp * this.roleplayApi.getMonsterXpBoostMultiplier(m.monsterId) * this.roleplayApi.getAlmanaxMonsterXpBonusMultiplier(m.monsterId),m.hiddenLevel));
            }
         }
         numFirstMonsters = formulaMobs.length;
         idolsExpBonusTotalPercentSolo = this.getIdolsExpBonusPercent(this.playerApi.getSoloIdols(),numFirstMonsters,grp,0);
         idolsExpBonusTotalPercentGroup = this.getIdolsExpBonusPercent(!this.playerApi.isInParty() && Boolean(pGroupMembersData.length) ? this.playerApi.getSoloIdols() : this.playerApi.getPartyIdols(),numFirstMonsters,grp,pGroupMembersData.length);
         FormulaHandler.getInstance().initXpFormula(pPlayerData,formulaMobs,pGroupMembersData,pMapRewardRate,idolsExpBonusTotalPercentSolo,idolsExpBonusTotalPercentGroup);
         return new MonstersGroupData(totalLevel,grp,FormulaHandler.getInstance().xpSolo,FormulaHandler.getInstance().xpGroup);
      }
      
      private function getXp(monsterData:*, monster:Monster, realMonsterGrade:int) : int
      {
         if((this.playerApi.isInAnomaly() || this._useScale) && monsterData.level > 0)
         {
            return this.utilApi.getCurrentMonsterXp(monsterData.level,monster.isBoss);
         }
         return realMonsterGrade <= 0 ? monster.grades[monsterData.grade - 1].gradeXp : monster.grades[realMonsterGrade - 1].gradeXp;
      }
      
      private function getIdolsExpBonusPercent(pIdolsList:Object, pNumFirstMonsters:uint, pMonsters:Array, pNbAllies:uint) : uint
      {
         var i:int = 0;
         var coeff:Number = NaN;
         var idol:Idol = null;
         var incompatibleIdol:Boolean = false;
         var monster:Monster = null;
         var m:Object = null;
         var numIdols:uint = uint(pIdolsList.length);
         var idolsExpBonusTotalPercent:uint = 0;
         var enabledIdols:Vector.<uint> = new Vector.<uint>(0);
         for(i = 0; i < numIdols; i++)
         {
            idol = this.dataApi.getIdol(pIdolsList[i]);
            incompatibleIdol = false;
            for each(m in pMonsters)
            {
               monster = this.dataApi.getMonsterFromId(m.monsterId);
               if(monster.incompatibleIdols.indexOf(idol.id) != -1)
               {
                  incompatibleIdol = true;
                  break;
               }
            }
            if(!(incompatibleIdol || idol.groupOnly && (pNbAllies < 4 || pNumFirstMonsters < 4)))
            {
               enabledIdols.push(pIdolsList[i]);
            }
         }
         numIdols = enabledIdols.length;
         for(i = 0; i < numIdols; i++)
         {
            idol = this.dataApi.getIdol(enabledIdols[i]);
            coeff = this.getIdolCoeff(idol,enabledIdols);
            idolsExpBonusTotalPercent += idol.experienceBonus * coeff;
         }
         return idolsExpBonusTotalPercent;
      }
      
      private function getIdolCoeff(pIdol:Idol, pIdolsList:Object) : Number
      {
         var i:int = 0;
         var j:int = 0;
         var coeff:Number = 1;
         var synergiesIds:Object = pIdol.synergyIdolsIds;
         var synergiesCoeffs:Object = pIdol.synergyIdolsCoeff;
         var numSynergies:uint = uint(synergiesIds.length);
         var numActiveIdols:uint = uint(pIdolsList.length);
         for(i = 0; i < numActiveIdols; i++)
         {
            for(j = 0; j < numSynergies; j++)
            {
               if(synergiesIds[j] == pIdolsList[i])
               {
                  coeff *= synergiesCoeffs[j];
               }
            }
         }
         return coeff;
      }
      
      public function unload() : void
      {
      }
   }
}

class MonstersGroupData
{
   
   private var _level:int;
   
   private var _group:Array;
   
   private var _xpSolo:Number;
   
   private var _xpGroup:Number;
   
   public function MonstersGroupData(pLevel:int, pGroup:Array, pXpSolo:Number, pXpGroup:Number)
   {
      super();
      this._level = pLevel;
      this._group = pGroup;
      this._xpSolo = pXpSolo;
      this._xpGroup = pXpGroup;
   }
   
   public function get level() : int
   {
      return this._level;
   }
   
   public function get group() : Array
   {
      return this._group;
   }
   
   public function get xpSolo() : Number
   {
      return this._xpSolo;
   }
   
   public function get xpGroup() : Number
   {
      return this._xpGroup;
   }
}
