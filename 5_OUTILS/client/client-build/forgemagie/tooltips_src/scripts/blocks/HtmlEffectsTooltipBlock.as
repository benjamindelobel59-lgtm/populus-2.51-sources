package blocks
{
   import blockParams.EffectsTooltipBlockParameters;
   import com.ankama.dofus.enums.ActionIds;
   import com.ankamagames.dofus.datacenter.effects.instances.EffectInstanceDice;
   import com.ankamagames.dofus.datacenter.effects.instances.EffectInstanceInteger;
   import com.ankamagames.dofus.datacenter.effects.instances.EffectInstanceMinMax;
   import com.ankamagames.dofus.datacenter.effects.instances.EffectInstanceMount;
   import com.ankamagames.dofus.datacenter.monsters.Monster;
   import com.ankamagames.dofus.datacenter.monsters.MonsterGrade;
   import com.ankamagames.dofus.datacenter.mounts.MountBehavior;
   import com.ankamagames.dofus.internalDatacenter.spells.SpellWrapper;
   import d2enums.DataEnum;
   
   public class HtmlEffectsTooltipBlock
   {
      
      public const INDEX_DAMAGES:uint = 0;
      
      public const INDEX_EFFECTS:uint = 1;
      
      public const INDEX_STATUS:uint = 2;
      
      private var _effect:Object;
      
      private var _content:String;
      
      private var _block:Object;
      
      private var _setInfo:String;
      
      private var _splitDamageAndEffects:Boolean;
      
      private var _itemTheoreticalEffects:Array;
      
      private var _showDamages:Boolean;
      
      private var _showTheoreticalEffects:Boolean;
      
      private var _addTheoricalEffects:Boolean;
      
      private var _isCriticalEffects:Boolean;
      
      private var _showLabel:Boolean;
      
      private var _showDuration:Boolean;
      
      private var _length:int;
      
      private var _exoticEffects:Array;
      
      private var _missingEffects:Array;
      
      private var _exoticDamage:Array;
      
      private var _missingDamage:Array;
      
      private var _regReplace:RegExp = /([0-9]+)/g;
      
      private var _customli:String = "customlirightmargin";
      
      public function HtmlEffectsTooltipBlock(params:EffectsTooltipBlockParameters)
      {
         var e:* = undefined;
         super();
         this._effect = params.effects;
         this._showDamages = params.showDamages;
         this._showTheoreticalEffects = params.showTheoreticalEffects;
         this._addTheoricalEffects = params.addTheoreticalEffects;
         this._isCriticalEffects = params.isCriticalEffects;
         this._length = params.length;
         this._showLabel = params.showLabel;
         this._showDuration = params.showDuration;
         this._splitDamageAndEffects = params.splitDamageAndEffects;
         this._customli = params.customli;
         if(params.itemTheoreticalEffects)
         {
            this._itemTheoreticalEffects = [];
            for each(e in params.itemTheoreticalEffects)
            {
               if(!this._itemTheoreticalEffects[e.effectId])
               {
                  this._itemTheoreticalEffects[e.effectId] = [];
               }
               if(e.effectId != ActionIds.ACTION_CHARACTER_LEARN_EMOTICON)
               {
                  this._itemTheoreticalEffects[e.effectId].push(e);
               }
            }
         }
         this._block = Api.tooltip.createTooltipBlock(this.onAllChunkLoaded,this.getContent);
         var chunkList:Array = new Array(Api.tooltip.createChunkData("separator",params.chunkType + "/base/separator.txt"),Api.tooltip.createChunkData("subTitle",params.chunkType + "/base/subTitle.txt"),Api.tooltip.createChunkData("effect",params.chunkType + "/effect/effect.txt"),Api.tooltip.createChunkData("subEffect",params.chunkType + "/effect/subEffect.txt"));
         if(params.setInfo)
         {
            this._setInfo = params.setInfo;
            chunkList.unshift(Api.tooltip.createChunkData("namelessContent",params.chunkType + "/text/namelessContent.txt"));
         }
         this._block.initChunk(chunkList);
      }
      
      public function onAllChunkLoaded() : void
      {
         var ei:* = undefined;
         var effectsPart:_EffectPart = null;
         var currentCategory:int = 0;
         var exotic:Boolean = false;
         var effectList:Array = null;
         var i:int = 0;
         var effects:Array = null;
         this._content = "";
         this._exoticEffects = new Array();
         this._missingEffects = new Array();
         this._exoticDamage = new Array();
         this._missingDamage = new Array();
         var category:Array = new Array();
         var effectsByEffectId:Array = new Array();
         for each(ei in this._effect)
         {
            if(!(ei.category == -1 || !ei.visibleInTooltip))
            {
               if(ei.effectId != ActionIds.ACTION_ITEM_CHANGE_DURABILITY)
               {
                  currentCategory = int(this.INDEX_EFFECTS);
                  if(ei.category == DataEnum.ACTION_TYPE_DAMAGES)
                  {
                     currentCategory = int(this.INDEX_DAMAGES);
                  }
                  else if(ei.category == DataEnum.ACTION_TYPE_STATUS)
                  {
                     currentCategory = int(this.INDEX_STATUS);
                  }
                  if(!category[currentCategory])
                  {
                     category[currentCategory] = new Array();
                  }
                  effectsByEffectId[ei.effectId] = ei;
                  if(this._itemTheoreticalEffects)
                  {
                     exotic = this._itemTheoreticalEffects[ei.effectId] == null;
                     if(exotic && Boolean(ei.showInSet))
                     {
                        if(ei.category == DataEnum.ACTION_TYPE_DAMAGES)
                        {
                           this._exoticDamage.push(ei);
                        }
                        else
                        {
                           this._exoticEffects.push(ei);
                        }
                        continue;
                     }
                  }
                  category[currentCategory].push(ei);
               }
            }
         }
         if(Boolean(this._itemTheoreticalEffects) && this._addTheoricalEffects)
         {
            for each(effectList in this._itemTheoreticalEffects)
            {
               for(i = 0; i < effectList.length; i++)
               {
                  ei = effectList[i];
                  if(!effectsByEffectId[ei.effectId] && Boolean(ei.showInSet))
                  {
                     if(ei.category == DataEnum.ACTION_TYPE_DAMAGES)
                     {
                        this._missingDamage.push(ei);
                     }
                     else
                     {
                        this._missingEffects.push(ei);
                     }
                  }
               }
            }
         }
         if(!category.length)
         {
            return;
         }
         var sortedCategory:Array = new Array();
         if(category[this.INDEX_STATUS])
         {
            sortedCategory.push(new _EffectPart(Api.ui.getText("ui.chat.status.title"),DataEnum.ACTION_TYPE_STATUS,category[this.INDEX_STATUS]));
         }
         if(this._splitDamageAndEffects)
         {
            if(Boolean(category[this.INDEX_DAMAGES]) && Boolean(this._exoticDamage) && Boolean(this._exoticDamage.length))
            {
               category[this.INDEX_DAMAGES] = this._exoticDamage.concat(category[this.INDEX_DAMAGES]);
            }
            if(Boolean(category[this.INDEX_DAMAGES]) && Boolean(this._missingDamage) && Boolean(this._missingDamage.length))
            {
               category[this.INDEX_DAMAGES] = category[this.INDEX_DAMAGES].concat(this._missingDamage);
            }
            if(Boolean(category[this.INDEX_DAMAGES]) && this._showDamages)
            {
               sortedCategory.push(new _EffectPart(this._isCriticalEffects ? Api.ui.getText("ui.common.criticalDamages") : Api.ui.getText("ui.stats.damagesBonus"),DataEnum.ACTION_TYPE_DAMAGES,category[this.INDEX_DAMAGES]));
            }
            if(Boolean(category[this.INDEX_EFFECTS]) && Boolean(this._exoticEffects) && Boolean(this._exoticEffects.length))
            {
               category[this.INDEX_EFFECTS] = this._exoticEffects.concat(category[this.INDEX_EFFECTS]);
            }
            if(Boolean(category[this.INDEX_EFFECTS]) && Boolean(this._missingEffects) && Boolean(this._missingEffects.length))
            {
               category[this.INDEX_EFFECTS] = category[this.INDEX_EFFECTS].concat(this._missingEffects);
            }
            if(category[this.INDEX_EFFECTS])
            {
               sortedCategory.push(new _EffectPart(Api.ui.processText(this._isCriticalEffects ? Api.ui.getText("ui.common.criticalEffects") : Api.ui.getText("ui.common.effects"),"",category[this.INDEX_EFFECTS].length == 1),DataEnum.ACTION_TYPE_CARACTERISTICS,category[this.INDEX_EFFECTS]));
            }
         }
         else
         {
            effects = [];
            if(category[this.INDEX_DAMAGES])
            {
               effects = category[this.INDEX_DAMAGES];
            }
            if(category[this.INDEX_EFFECTS])
            {
               effects = effects.concat(category[this.INDEX_EFFECTS]);
            }
            sortedCategory.push(new _EffectPart(Api.ui.processText(this._isCriticalEffects ? Api.ui.getText("ui.common.criticalEffects") : Api.ui.getText("ui.common.effects"),"",effects.length == 1),DataEnum.ACTION_TYPE_CARACTERISTICS,effects));
         }
         if(this._setInfo)
         {
            this._content += this._block.getChunk("namelessContent").processContent({
               "content":this._setInfo,
               "css":"[local.css]tooltip_monster.css"
            });
         }
         var lastDuration:uint = 0;
         var starting:Boolean = true;
         for each(effectsPart in sortedCategory)
         {
            if(this._showLabel && Boolean(effectsPart.title))
            {
               if(!starting)
               {
                  this._content += this._block.getChunk("separator").processContent({});
               }
               this._content += this._block.getChunk("subTitle").processContent({"text":effectsPart.title});
            }
            for each(ei in effectsPart.effects)
            {
               if(ei)
               {
                  this._content += this.processEffect(effectsPart,ei,"effect");
                  lastDuration = uint(ei.duration);
               }
            }
            starting = false;
         }
         this._exoticEffects = null;
         this._missingEffects = null;
         this._exoticDamage = null;
         this._missingDamage;
         this._itemTheoreticalEffects = null;
      }
      
      private function processEffect(effectsPart:_EffectPart, ei:Object, chunk:String, chunkArgs:Object = null, showSubEffect:Boolean = true) : String
      {
         var spell:SpellWrapper = null;
         var theoreticalEffect:* = undefined;
         var theoreticalDesc:String = null;
         var myPattern:RegExp = null;
         var result:Object = null;
         var monster:Monster = null;
         var gradeId:int = 0;
         var grade:MonsterGrade = null;
         var level:int = 0;
         var lifePoints:int = 0;
         var bonusDodge:int = 0;
         var subSpell:Object = null;
         var bombData:Object = null;
         var subEffect:Object = null;
         var mountEffect:EffectInstanceMount = null;
         var nameText:String = null;
         var levelText:String = null;
         var sexText:String = null;
         var isRideableText:String = null;
         var fecondationStateText:String = null;
         var reproductionText:String = null;
         var capacitiesCount:int = 0;
         var capacitiesText:String = null;
         var ownerText:String = null;
         var now:Date = null;
         var expirationDateText:String = null;
         var subEffectMount:EffectInstanceInteger = null;
         var capacity:MountBehavior = null;
         var i:int = 0;
         if(!ei)
         {
            return "";
         }
         var content:String = "";
         var description:String = "";
         if(ei.effectId == ActionIds.ACTION_CAST_STARTING_SPELL)
         {
            spell = Api.data.getSpellWrapper(ei.diceNum,ei.diceSide);
            description += spell.spell.description;
         }
         else if(this._showTheoreticalEffects)
         {
            description += ei.showInSet ? ei.theoreticalDescriptionForTooltip : ei.theoreticalDescription;
         }
         else
         {
            description += ei.showInSet ? ei.descriptionForTooltip : ei.description;
         }
         if(!description || description == "null")
         {
            return "";
         }
         var cssClass:String = "bonus";
         var baseDescription:String = ei.description;
         if(effectsPart.type == DataEnum.ACTION_TYPE_CARACTERISTICS)
         {
            if(ei.bonusType == -1 || Boolean(baseDescription) && Boolean(baseDescription.charAt(0) == "-"))
            {
               if(description.indexOf("<span class=\'#valueCssClass\'>-") == 0)
               {
                  description = description.replace("<span class=\'#valueCssClass\'>-","<span class=\'#valueCssClass\'>- ");
               }
               cssClass = "malus";
            }
         }
         if(Boolean(this._exoticEffects) && Boolean(this._exoticEffects.indexOf(ei) != -1) || Boolean(this._exoticDamage) && Boolean(this._exoticDamage.indexOf(ei) != -1))
         {
            cssClass = "exotic";
         }
         else if(Boolean(this._missingEffects) && Boolean(this._missingEffects.indexOf(ei) != -1) || Boolean(this._missingDamage) && Boolean(this._missingDamage.indexOf(ei) != -1))
         {
            cssClass = "theoretical";
         }
         var valueCssClass:String = cssClass + "value";
         var duration:String = null;
         if(this._showDuration)
         {
            if(ei.durationString)
            {
               duration = " (" + ei.durationString + ")";
            }
         }
         if(ei.trigger)
         {
            description = Api.ui.getText("ui.spell.trigger",description);
         }
         if(duration)
         {
            description += duration;
         }
         if(Boolean(this._addTheoricalEffects && ei.showInSet && (!this._missingEffects || this._missingEffects.indexOf(ei) == -1)) && Boolean(this._itemTheoreticalEffects) && Boolean(this._itemTheoreticalEffects[ei.effectId]) && Boolean(this._itemTheoreticalEffects[ei.effectId].length))
         {
            theoreticalEffect = this._itemTheoreticalEffects[ei.effectId][0];
            if(Boolean(theoreticalEffect) && (theoreticalEffect is EffectInstanceMinMax || theoreticalEffect is EffectInstanceDice))
            {
               if(effectsPart.type != DataEnum.ACTION_TYPE_CARACTERISTICS || theoreticalEffect.description != ei.description)
               {
                  theoreticalDesc = theoreticalEffect.theoreticalShortDescriptionForTooltip;
                  if(theoreticalDesc)
                  {
                     description += "  <span class=\'theoretical\'>&#91; " + theoreticalDesc + " &#93;</span>";
                  }
               }
            }
         }
         if(Boolean(ei.targetMask) && Boolean(ei.targetMask.length) && (ei.targetMask.indexOf("i") != -1 || ei.targetMask.indexOf("s") != -1 || ei.targetMask.indexOf("I") != -1 || ei.targetMask.indexOf("S") != -1 || ei.targetMask.indexOf("j") != -1 || ei.targetMask.indexOf("J") != -1))
         {
            myPattern = new RegExp(/^[iIsSjJfFeE0-9,]+$/);
            result = myPattern.exec(ei.targetMask);
            if(result)
            {
               description += " (" + Api.ui.getText("ui.common.summon") + ")";
            }
         }
         if(!chunkArgs)
         {
            chunkArgs = new Object();
         }
         chunkArgs.text = description;
         chunkArgs.cssClass = cssClass;
         chunkArgs.li = "";
         chunkArgs.customli = this._customli;
         if(chunk == "subEffect")
         {
            content += this.processSubEffectChunk(chunkArgs,{"valueCssClass":valueCssClass});
         }
         else
         {
            content += this._block.getChunk(chunk).processContent(chunkArgs,{"valueCssClass":valueCssClass});
         }
         if(showSubEffect)
         {
            if(ei.effectId == ActionIds.ACTION_SUMMON_CREATURE || ei.effectId == ActionIds.ACTION_FIGHT_KILL_AND_SUMMON || ei.effectId == ActionIds.ACTION_FIGHT_KILL_AND_SUMMON_SLAVE || ei.effectId == ActionIds.ACTION_SUMMON_SLAVE)
            {
               monster = Api.data.getMonsterFromId(int(ei.parameter0));
               if(monster)
               {
                  gradeId = int(ei.parameter1);
                  if(gradeId < 1 || gradeId > monster.grades.length)
                  {
                     gradeId = int(monster.grades.length);
                  }
                  grade = monster.grades[gradeId - 1];
                  level = 1;
                  if(Api.player.getPlayedCharacterInfo())
                  {
                     level = int(Api.player.getPlayedCharacterInfo().limitedLevel);
                  }
                  lifePoints = Math.floor(grade.lifePoints + grade.lifePoints * level / 100);
                  content += this._block.getChunk("subEffect").processContent({
                     "text":Api.ui.getText("ui.stats.HP") + Api.ui.getText("ui.common.colon") + "<span class=\'#valueCssClass\'>" + lifePoints + "</span>",
                     "rightText":Api.ui.getText("ui.stats.neutralReductionPercent") + Api.ui.getText("ui.common.colon") + "<span class=\'#valueCssClass\'>" + grade.neutralResistance + "</span>"
                  },{"valueCssClass":"value"});
                  content += this._block.getChunk("subEffect").processContent({
                     "text":Api.ui.getText("ui.stats.shortAP") + Api.ui.getText("ui.common.colon") + "<span class=\'#valueCssClass\'>" + grade.actionPoints + "</span>",
                     "rightText":Api.ui.getText("ui.stats.earthReductionPercent") + Api.ui.getText("ui.common.colon") + "<span class=\'#valueCssClass\'>" + grade.earthResistance + "</span>"
                  },{"valueCssClass":"value"});
                  content += this._block.getChunk("subEffect").processContent({
                     "text":Api.ui.getText("ui.stats.shortMP") + Api.ui.getText("ui.common.colon") + "<span class=\'#valueCssClass\'>" + grade.movementPoints + "</span>",
                     "rightText":Api.ui.getText("ui.stats.fireReductionPercent") + Api.ui.getText("ui.common.colon") + "<span class=\'#valueCssClass\'>" + grade.fireResistance + "</span>"
                  },{"valueCssClass":"value"});
                  bonusDodge = Math.floor((grade.wisdom + grade.wisdom * level / 100) / 10);
                  content += this._block.getChunk("subEffect").processContent({
                     "text":Api.ui.getText("ui.stats.dodgeAP") + Api.ui.getText("ui.common.colon") + "<span class=\'#valueCssClass\'>" + (grade.paDodge + bonusDodge) + "</span>",
                     "rightText":Api.ui.getText("ui.stats.waterReductionPercent") + Api.ui.getText("ui.common.colon") + "<span class=\'#valueCssClass\'>" + grade.waterResistance + "</span>"
                  },{"valueCssClass":"value"});
                  content += this._block.getChunk("subEffect").processContent({
                     "text":Api.ui.getText("ui.stats.dodgeMP") + Api.ui.getText("ui.common.colon") + "<span class=\'#valueCssClass\'>" + (grade.pmDodge + bonusDodge) + "</span>",
                     "rightText":Api.ui.getText("ui.stats.airReductionPercent") + Api.ui.getText("ui.common.colon") + "<span class=\'#valueCssClass\'>" + grade.airResistance + "</span>"
                  },{"valueCssClass":"value"});
               }
            }
            if(ei.effectId == ActionIds.ACTION_FIGHT_ADD_TRAP_CASTING_SPELL || ei.effectId == ActionIds.ACTION_FIGHT_ADD_GLYPH_CASTING_SPELL || ei.effectId == ActionIds.ACTION_SUMMON_BOMB || ei.effectId == ActionIds.ACTION_FIGHT_ADD_GLYPH_CASTING_SPELL_ENDTURN || ei.effectId == ActionIds.ACTION_FIGHT_ADD_GLYPH_AURA)
            {
               if(ei.effectId != ActionIds.ACTION_SUMMON_BOMB)
               {
                  subSpell = Api.data.getSpellWrapper(int(ei.parameter0),int(ei.parameter1));
               }
               else
               {
                  bombData = Api.data.getBomb(int(ei.parameter0));
                  subSpell = Api.data.getSpellWrapper(bombData.explodSpellId,int(ei.parameter1));
               }
               if(subSpell)
               {
                  for each(subEffect in subSpell.effects)
                  {
                     if(subEffect.visibleInTooltip)
                     {
                        content += this.processEffect(effectsPart,subEffect,"subEffect",{"rightText":""},false);
                     }
                  }
               }
            }
            if(ei.effectId == ActionIds.ACTION_RIDE_DETAILS)
            {
               mountEffect = ei as EffectInstanceMount;
               if(mountEffect.name == "")
               {
                  nameText = Api.ui.getText("ui.common.noName");
               }
               else
               {
                  nameText = Api.ui.getText("ui.common.name") + Api.ui.getText("ui.common.colon") + "<span class=\'#valueCssClass\'>" + mountEffect.name + "</span>";
               }
               levelText = Api.ui.getText("ui.common.level") + Api.ui.getText("ui.common.colon") + "<span class=\'#valueCssClass\'>" + mountEffect.level + "</span>";
               sexText = mountEffect.sex ? Api.ui.getText("ui.common.animalFemale") : Api.ui.getText("ui.common.animalMale");
               sexText = "<span class=\'#valueCssClass\'>" + sexText + "</span>";
               isRideableText = Api.ui.getText("ui.common.mountable") + Api.ui.getText("ui.common.colon") + (mountEffect.isRideable ? "<span class=\'#valueCssClass\'>" + Api.ui.getText("ui.common.yes") + "</span>" : Api.ui.getText("ui.common.no"));
               fecondationStateText = "<span class=\'#valueCssClass\'>";
               if(mountEffect.isFeconded)
               {
                  fecondationStateText += Api.ui.getText("ui.mount.filterFecondee");
               }
               else if(mountEffect.isFecondationReady)
               {
                  fecondationStateText += Api.ui.getText("ui.mount.filterFecondable");
               }
               else
               {
                  fecondationStateText += Api.ui.getText("ui.mount.filterNoFecondable");
               }
               fecondationStateText += "</span>";
               if(mountEffect.reproductionCount == -1)
               {
                  reproductionText = Api.ui.getText("ui.mount.castrated");
               }
               else if(mountEffect.reproductionCount >= mountEffect.reproductionCountMax)
               {
                  reproductionText = Api.ui.getText("ui.mount.sterilized");
               }
               else
               {
                  reproductionText = Api.ui.getText("ui.common.reproductions") + Api.ui.getText("ui.common.colon") + "<span class=\'#valueCssClass\'>" + mountEffect.reproductionCount + " / " + mountEffect.reproductionCountMax + "</span>";
               }
               capacitiesCount = int(mountEffect.capacities.length);
               capacitiesText = "";
               if(capacitiesCount > 0)
               {
                  capacity = Api.data.getMountBehaviorById(mountEffect.capacities[0]);
                  capacitiesText += capacity.name;
                  if(capacitiesCount > 1)
                  {
                     for(i = 1; i < capacitiesCount; )
                     {
                        capacity = Api.data.getMountBehaviorById(mountEffect.capacities[i]);
                        capacitiesText += ", " + capacity.name;
                        i++;
                     }
                  }
                  capacitiesText = "<span class=\'#valueCssClass\'>" + capacitiesText + "</span>";
               }
               ownerText = Api.ui.getText("ui.common.belongsTo") + Api.ui.getText("ui.common.colon") + mountEffect.owner;
               now = new Date();
               expirationDateText = Api.ui.getText("ui.mount.expiration",Api.time.getShortDuration(mountEffect.expirationDate - now.time,false));
               content += this._block.getChunk("subEffect").processContent({
                  "text":nameText,
                  "rightText":levelText
               },{"valueCssClass":"value"});
               content += this._block.getChunk("subEffect").processContent({
                  "text":sexText,
                  "rightText":isRideableText
               },{"valueCssClass":"value"});
               content += this._block.getChunk("subEffect").processContent({
                  "text":fecondationStateText,
                  "rightText":reproductionText
               },{"valueCssClass":"value"});
               content += this._block.getChunk("subEffect").processContent({
                  "text":capacitiesText,
                  "rightText":""
               },{"valueCssClass":"value"});
               content += this._block.getChunk("subEffect").processContent({
                  "text":ownerText,
                  "rightText":""
               });
               content += this._block.getChunk("subEffect").processContent({
                  "text":expirationDateText,
                  "rightText":""
               });
               for each(subEffectMount in mountEffect.effects)
               {
                  content += this.processEffect(effectsPart,subEffectMount,"subEffect",{"rightText":""},false);
               }
            }
         }
         return content;
      }
      
      private function getMaxWordwrapIndex(text:String, maxLength:int) : int
      {
         var tmpIndex:int = 0;
         var i:* = 0;
         var index:int = Math.min(maxLength,text.length);
         if(index == maxLength)
         {
            tmpIndex = index - 1;
            if(text.charAt(tmpIndex) != " ")
            {
               for(i = int(tmpIndex - 1); i >= 0; i--)
               {
                  if(text.charAt(i) == " ")
                  {
                     index = i + 1;
                     break;
                  }
               }
            }
         }
         return index;
      }
      
      private function processSubEffectChunk(chunkArgs:Object, secondaryParams:Object = null, forcedMaxLength:uint = 0) : String
      {
         var maxLength:uint = 0;
         var openingValueSpanTag:String = null;
         var closingSpanTag:String = null;
         var cssClass:String = null;
         var valueText:String = null;
         var openingBonusValueTag:String = null;
         var startIndex:int = 0;
         var endIndex:int = 0;
         var bonusValueText:String = null;
         var remainingText:String = null;
         var leftPart:String = null;
         var rightPart:String = null;
         var middlePart:String = null;
         var textPart:String = null;
         var index:int = 0;
         if(forcedMaxLength != 0)
         {
            maxLength = forcedMaxLength;
         }
         else if(Api.system.getActiveFontType() == "default")
         {
            maxLength = 47;
         }
         else
         {
            maxLength = 45;
         }
         var processedContent:String = "";
         var text:String = chunkArgs.text;
         if(text.indexOf("#valueCssClass") != -1)
         {
            openingValueSpanTag = "<span class=\'#valueCssClass\'>";
            closingSpanTag = "</span>";
            cssClass = chunkArgs.cssClass;
            valueText = text.substring(0,text.lastIndexOf(closingSpanTag));
            while(valueText.indexOf(openingValueSpanTag) != -1)
            {
               valueText = valueText.replace(openingValueSpanTag,"");
            }
            while(valueText.indexOf(closingSpanTag) != -1)
            {
               valueText = valueText.replace(closingSpanTag,"");
            }
            chunkArgs.text = valueText;
            chunkArgs.cssClass = secondaryParams["valueCssClass"];
            processedContent += this.processSubEffectChunk(chunkArgs,null,Api.system.getActiveFontType() == "smallScreen" ? 40 : 0);
            openingBonusValueTag = "<p class=\'subeffectleftcolumn\'><span class=\'" + chunkArgs.cssClass + "\'>";
            startIndex = processedContent.lastIndexOf(openingBonusValueTag) + openingBonusValueTag.length;
            endIndex = processedContent.indexOf(closingSpanTag,startIndex);
            bonusValueText = processedContent.substring(startIndex,endIndex);
            startIndex = text.lastIndexOf(closingSpanTag) + closingSpanTag.length;
            remainingText = text.substring(startIndex);
            leftPart = processedContent.substring(0,endIndex + closingSpanTag.length);
            rightPart = processedContent.substring(endIndex + closingSpanTag.length);
            endIndex = this.getMaxWordwrapIndex(remainingText,maxLength - bonusValueText.length);
            middlePart = remainingText.substring(0,endIndex);
            processedContent = leftPart + "<span class=\'" + cssClass + "\'>" + middlePart + "</span>" + rightPart;
            chunkArgs.text = remainingText.substring(endIndex);
            if(chunkArgs.text.length)
            {
               chunkArgs.cssClass = cssClass;
               processedContent += this.processSubEffectChunk(chunkArgs);
            }
            return processedContent;
         }
         if(text.length > maxLength)
         {
            while(text.length)
            {
               textPart = "";
               index = this.getMaxWordwrapIndex(text,maxLength);
               textPart = text.substring(0,index);
               chunkArgs.text = textPart;
               processedContent += this._block.getChunk("subEffect").processContent(chunkArgs,secondaryParams);
               text = text.substring(index);
            }
         }
         else
         {
            processedContent = this._block.getChunk("subEffect").processContent(chunkArgs,secondaryParams);
         }
         return processedContent;
      }
      
      public function getContent() : String
      {
         return this._content;
      }
      
      public function get block() : Object
      {
         return this._block;
      }
   }
}

class _EffectPart
{
   
   public var title:String;
   
   public var type:uint;
   
   public var effects:Array;
   
   public function _EffectPart(title:String, type:uint, effects:Array)
   {
      super();
      this.title = title;
      this.type = type;
      this.effects = effects;
   }
}
