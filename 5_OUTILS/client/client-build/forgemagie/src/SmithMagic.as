package ui
{
   import com.ankama.dofus.enums.ActionIds;
   import com.ankamagames.berilia.api.UiApi;
   import com.ankamagames.berilia.components.EntityDisplayer;
   import com.ankamagames.berilia.components.Grid;
   import com.ankamagames.berilia.components.Label;
   import com.ankamagames.berilia.components.Slot;
   import com.ankamagames.berilia.components.TextArea;
   import com.ankamagames.berilia.components.Texture;
   import com.ankamagames.berilia.types.graphic.ButtonContainer;
   import com.ankamagames.dofus.datacenter.effects.EffectInstance;
   import com.ankamagames.dofus.datacenter.effects.instances.EffectInstanceDice;
   import com.ankamagames.dofus.datacenter.effects.instances.EffectInstanceInteger;
   import com.ankamagames.dofus.datacenter.effects.instances.EffectInstanceMinMax;
   import com.ankamagames.dofus.datacenter.items.Item;
   import com.ankamagames.dofus.internalDatacenter.items.ItemWrapper;
   import com.ankamagames.dofus.uiApi.ContextMenuApi;
   import com.ankamagames.dofus.uiApi.DataApi;
   import com.ankamagames.dofus.uiApi.InventoryApi;
   import com.ankamagames.dofus.uiApi.JobsApi;
   import com.ankamagames.dofus.uiApi.PlayedCharacterApi;
   import com.ankamagames.dofus.uiApi.SocialApi;
   import com.ankamagames.dofus.uiApi.SoundApi;
   import com.ankamagames.dofus.uiApi.StorageApi;
   import com.ankamagames.dofus.uiApi.SystemApi;
   import com.ankamagames.dofus.uiApi.UtilApi;
   import com.ankamagames.dofusModuleLibrary.enum.SoundEnum;
   import com.ankamagames.dofusModuleLibrary.enum.SoundTypeEnum;
   import d2actions.*;
   import d2enums.ComponentHookList;
   import d2enums.DataEnum;
   import d2enums.ProtocolConstantsEnum;
   import d2hooks.*;
   import flash.events.TimerEvent;
   import flash.geom.ColorTransform;
   import flash.text.AntiAliasType;
   import flash.ui.Keyboard;
   import flash.utils.Timer;
   import flash.utils.getTimer;
   
   public class SmithMagic
   {
      
      public static const TOOLTIP_SMITH_MAGIC:String = "tooltipSmithMagic";
      
      public static const CRAFT_IMPOSSIBLE:int = 0;
      
      public static const CRAFT_FAILED:int = 1;
      
      public static const CRAFT_SUCCESS:int = 2;
      
      public static const CRAFT_NEARLY_SUCCESS:int = 3;
      
      protected static const ICON_CT:ColorTransform = new ColorTransform(1,1,1,0.3,179,179,177);
      
      protected static const DEFAULT_CT:ColorTransform = new ColorTransform();
      
      public var sysApi:SystemApi;
      
      public var uiApi:UiApi;
      
      [Module(name="Ankama_Common")]
      public var modCommon:Object;
      
      [Module(name="Ankama_ContextMenu")]
      public var modContextMenu:Object;
      
      public var socialApi:SocialApi;
      
      public var dataApi:DataApi;
      
      public var playerApi:PlayedCharacterApi;
      
      public var jobsApi:JobsApi;
      
      public var menuApi:ContextMenuApi;
      
      public var soundApi:SoundApi;
      
      public var storageApi:StorageApi;
      
      public var inventoryApi:InventoryApi;
      
      public var utilApi:UtilApi;
      
      protected var _skill:Object;
      
      protected var _job:*;
      
      protected var _skillLevel:int;
      
      protected var _itemToMage:Object;
      
      protected var _itemEffects:Array = new Array();
      
      protected var _newItem:Boolean;
      
      protected var _effectsDeltas:Object;
      
      protected var _smithForbidden:Boolean = false;
      
      protected var _refill_item:Object = null;
      
      protected var _refill_qty:int;
      
      protected var _lastScrollbarValue:int;
      
      protected var _corruptionOrTranscendanceRuneUsed:Boolean = false;
      
      protected var _waitingObject:Object;
      
      protected var _waitingSlot:Object;
      
      protected var _altClickedSlot:Object;
      
      protected var _bagSlotTexture:Object;
      
      protected var _multiCraft:Boolean = false;
      
      protected var _runesItemTypes:Array = [DataEnum.ITEM_TYPE_SMITHMAGIC_RUNE,DataEnum.ITEM_TYPE_SMITHMAGIC_TRANSCENDANCE_RUNE,DataEnum.ITEM_TYPE_SMITHMAGIC_CORRUPTION_RUNE];
      
      protected var _runesFromInventoryByEffectId:Array = new Array();
      
      protected var _effectIdForRunesUpdate:int = -1;
      
      protected var _runeTypeUpdate:int = -1;
      
      protected var _keepScrollbar:Boolean;
      
      private var _mergeButtonTimer:Timer;
      
      private var _mergeButtonTimerOut:Boolean;
      
      private var _mergeResultGot:Boolean;
      
      protected var _moveRequestedItemUid:int;
      
      protected var _popupName:String;
      
      public var ed_rightCharacter:EntityDisplayer;
      
      public var lbl_job:Label;
      
      public var gd_itemEffects:Grid;
      
      public var lbl_title:Label;
      
      public var texta_log:TextArea;
      
      public var slot_item:Slot;
      
      public var slot_rune:Slot;
      
      public var slot_sign:Slot;
      
      public var tx_slot_sign:Texture;
      
      public var btn_mergeAll:ButtonContainer;
      
      public var btn_lbl_btn_mergeAll:Label;
      
      public var btn_mergeOnce:ButtonContainer;
      
      public var btn_close:ButtonContainer;

      public var lbl_reliquat:Label;

      public var btn_modeAvance:ButtonContainer;

      public var btn_viderHistorique:ButtonContainer;

      protected var _reliquatObjetUid:int = -1;

      protected var _reliquatValeur:int = 0;

      protected var _reliquatDelta:int = 0;

      protected var _resultatFmAttendu:Boolean = false;

      protected var _modeAvance:Boolean = false;

      protected var _fusionAvanceeGid:int = 0;

      protected var _idsTheoriques:Array = new Array();
      
      protected var timer:int = getTimer();
      
      public function SmithMagic()
      {
         super();
      }
      
      public function main(args:Object) : void
      {
         var slot:Slot = null;
         var logs:Array = null;
         var jobExperience:Object = null;
         var color:Object = null;
         var log:Object = null;
         this.btn_mergeAll.soundId = SoundEnum.OK_BUTTON;
         this.btn_mergeOnce.soundId = SoundEnum.OK_BUTTON;
         this.sysApi.disableWorldInteraction();
         this.sysApi.addHook(d2hooks.JobLevelUp,this.onJobLevelUp);
         this.sysApi.addHook(ExchangeObjectModified,this.onExchangeObjectModified);
         this.sysApi.addHook(ExchangeObjectAdded,this.onExchangeObjectAdded);
         this.sysApi.addHook(ExchangeObjectRemoved,this.onExchangeObjectRemoved);
         this.sysApi.addHook(ExchangeLeave,this.onExchangeLeave);
         this.sysApi.addHook(DropStart,this.onDropStart);
         this.sysApi.addHook(DropEnd,this.onDropEnd);
         this.sysApi.addHook(ExchangeCraftResult,this.onExchangeCraftResult);
         this.sysApi.addHook(ExchangeItemAutoCraftStoped,this.onExchangeItemAutoCraftStoped);
         this.sysApi.addHook(ItemMagedResult,this.onItemMagedResult);
         this.sysApi.addHook(DoubleClickItemInventory,this.onDoubleClickItemInventory);
         this.sysApi.addHook(MouseCtrlDoubleClick,this.onMouseCtrlDoubleClick);
         this.sysApi.addHook(MouseAltDoubleClick,this.onMouseAltDoubleClick);
         this.sysApi.addHook(ObjectDeleted,this.onObjectDeleted);
         this.sysApi.addHook(ObjectQuantity,this.onObjectQuantity);
         this.uiApi.addComponentHook(this.ed_rightCharacter,ComponentHookList.ON_RELEASE);
         this.uiApi.addComponentHook(this.ed_rightCharacter,ComponentHookList.ON_RIGHT_CLICK);
         this.texta_log.antialias = AntiAliasType.ADVANCED;
         this._bagSlotTexture = this.uiApi.createUri(this.uiApi.me().getConstant("bagSlot_uri"));
         this._skill = this.jobsApi.getSkillFromId(args.skillId);
         this._job = this._skill.parentJob;
         if(Boolean(args.crafterInfos) && args.crafterInfos.id != this.playerApi.id())
         {
            this._skillLevel = args.crafterInfos.skillLevel;
         }
         else
         {
            jobExperience = this.jobsApi.getJobExperience(this._job.id);
            this._skillLevel = jobExperience.currentLevel;
         }
         this.lbl_title.text = this._skill.name;
         this.setMergeButtonDisabled(true);
         this.slot_item.emptyTexture = this.uiApi.createUri(this.uiApi.me().getConstant(this.pictoNameFromSkillId(this._skill.id)));
         this.slot_item.iconColorTransform = ICON_CT;
         this.slot_rune.emptyTexture = this.uiApi.createUri(this.uiApi.me().getConstant("rune_slot_uri"));
         this.slot_rune.iconColorTransform = ICON_CT;
         if(this._skillLevel < ProtocolConstantsEnum.MAX_JOB_LEVEL)
         {
            color = this.uiApi.getColor(this.sysApi.getConfigEntry("colors.smithmagic.fail"));
            this.tx_slot_sign.colorTransform = new ColorTransform(1,1,1,1,color.red,color.green,color.blue);
            this.slot_sign.highlightTexture = null;
         }
         this.slot_sign.emptyTexture = this.uiApi.createUri(this.uiApi.me().getConstant("signature_slot_uri"));
         this.slot_sign.iconColorTransform = ICON_CT;
         this.slot_item.refresh();
         this.slot_rune.refresh();
         this.slot_sign.refresh();
         for each(slot in [this.slot_item,this.slot_rune,this.slot_sign])
         {
            slot.dropValidator = this.dropValidatorFunction as Function;
            slot.processDrop = this.processDropFunction as Function;
            this.uiApi.addComponentHook(slot,"onRollOver");
            this.uiApi.addComponentHook(slot,"onRollOut");
            this.uiApi.addComponentHook(slot,"onDoubleClick");
            this.uiApi.addComponentHook(slot,"onRightClick");
            this.uiApi.addComponentHook(slot,"onRelease");
         }
         this.ed_rightCharacter.direction = 3;
         this.ed_rightCharacter.look = this.playerApi.getPlayedCharacterInfo().entityLook;
         this.lbl_job.text = this._job.name + " " + this.uiApi.getText("ui.common.short.level") + " " + this._skillLevel;
         this._mergeButtonTimer = new Timer(400,1);
         this._mergeButtonTimer.addEventListener(TimerEvent.TIMER_COMPLETE,this.onMergeButtonTimer);
         this._mergeButtonTimerOut = false;
         this._mergeResultGot = false;
         this.gd_itemEffects.dataProvider = new Array();
         logs = Job.getInstance().mageLog;
         if(Boolean(logs) && Boolean(logs.length))
         {
            for each(log in logs)
            {
               this.texta_log.appendText(log.text,log.cssClass);
            }
         }
         this.texta_log.scrollV = this.texta_log.maxScrollV;
         this.getRunesFromInventory();
         this._modeAvance = this.sysApi.getData("mystic_fm_mode_avance") == true;
         this.btn_modeAvance.selected = this._modeAvance;
         this.lbl_reliquat.text = "";
      }
      
      public function unload() : void
      {
         this._popupName = null;
         this.sysApi.enableWorldInteraction();
         this._mergeButtonTimer.stop();
         this._mergeButtonTimer.removeEventListener(TimerEvent.TIMER_COMPLETE,this.onMergeButtonTimer);
         this.uiApi.unloadUi("itemBoxSmith");
         this.storageApi.removeAllItemMasks("smithMagic");
         this.storageApi.releaseHooks();
         this.sysApi.sendAction(new LeaveDialogRequest());
         this.sysApi.sendAction(new CloseInventory());
      }
      
      public function get skill() : Object
      {
         return this._skill;
      }
      
      public function updateEffectLine(data:*, componentsRef:*, selected:Boolean) : void
      {
         var runes:Object = null;
         var i:int = 0;
         if(data)
         {
            if(!data.effect)
            {
               componentsRef.lbl_effect.cssClass = data.descZero != "" ? "smithmagiczero" : "p4";
            }
            else if(data.max != int.MAX_VALUE && data.effect.bonusType != -1 && data.effect is EffectInstanceInteger && int(EffectInstanceInteger(data.effect).value) > int(data.max))
            {
               componentsRef.lbl_effect.cssClass = "smithmagicover";
            }
            else if(data.descZero == "")
            {
               componentsRef.lbl_effect.cssClass = "smithmagicexo";
            }
            else if(data.effect.bonusType == -1)
            {
               componentsRef.lbl_effect.cssClass = "smithmagicfail";
            }
            else if(data.effect.bonusType == 1)
            {
               componentsRef.lbl_effect.cssClass = "smithmagicsuccess";
            }
            else
            {
               componentsRef.lbl_effect.cssClass = "p";
            }
            if(data.min == int.MIN_VALUE)
            {
               componentsRef.lbl_min.text = "-";
            }
            else
            {
               componentsRef.lbl_min.text = data.min;
            }
            if(data.max == int.MAX_VALUE)
            {
               componentsRef.lbl_max.text = "-";
            }
            else
            {
               componentsRef.lbl_max.text = data.max;
            }
            if(data.effect)
            {
               componentsRef.lbl_effect.text = data.effect.description;
            }
            else
            {
               componentsRef.lbl_effect.text = data.descZero;
            }
            if(data.delta > 0)
            {
               componentsRef.lbl_change.text = "+" + data.delta;
               componentsRef.ctr_effectLine.bgColor = this.sysApi.getConfigEntry("colors.smithmagic.success");
               componentsRef.ctr_effectLine.bgAlpha = 0.8;
            }
            else if(data.delta < 0)
            {
               componentsRef.lbl_change.text = "" + data.delta;
               componentsRef.ctr_effectLine.bgColor = this.sysApi.getConfigEntry("colors.smithmagic.fail");
               componentsRef.ctr_effectLine.bgAlpha = 0.8;
            }
            else
            {
               componentsRef.lbl_change.text = "";
               componentsRef.ctr_effectLine.bgAlpha = 0;
            }
            runes = this.getRunesByEffectId(data.id);
            if(runes)
            {
               for(i = 0; i < 3; i++)
               {
                  componentsRef["slot_rune" + i].data = runes[i].rune;
                  if(Boolean(runes[i].rune) && Boolean(runes[i].fromBag))
                  {
                     componentsRef["slot_rune" + i].customTexture = this._bagSlotTexture;
                  }
                  else
                  {
                     componentsRef["slot_rune" + i].customTexture = null;
                  }
                  componentsRef["slot_rune" + i].dropValidator = this.dropValidatorFunction as Function;
                  componentsRef["slot_rune" + i].processDrop = this.processDropFunction as Function;
                  this.uiApi.addComponentHook(componentsRef["slot_rune" + i],"onRollOver");
                  this.uiApi.addComponentHook(componentsRef["slot_rune" + i],"onRollOut");
                  this.uiApi.addComponentHook(componentsRef["slot_rune" + i],"onDoubleClick");
                  this.uiApi.addComponentHook(componentsRef["slot_rune" + i],"onRightClick");
                  this.uiApi.addComponentHook(componentsRef["slot_rune" + i],"onRelease");
               }
            }
            else
            {
               if(componentsRef.slot_rune0.data)
               {
                  componentsRef.slot_rune0.data = null;
               }
               if(componentsRef.slot_rune1.data)
               {
                  componentsRef.slot_rune1.data = null;
               }
               if(componentsRef.slot_rune2.data)
               {
                  componentsRef.slot_rune2.data = null;
               }
            }
         }
         else
         {
            componentsRef.lbl_min.text = "";
            componentsRef.lbl_max.text = "";
            componentsRef.lbl_effect.text = "";
            componentsRef.lbl_change.text = "";
            componentsRef.slot_rune0.data = null;
            componentsRef.slot_rune1.data = null;
            componentsRef.slot_rune2.data = null;
            componentsRef.slot_rune0.customTexture = null;
            componentsRef.slot_rune1.customTexture = null;
            componentsRef.slot_rune2.customTexture = null;
            componentsRef.lbl_cheat.text = "";
            componentsRef.ctr_effectLine.bgAlpha = 0;
         }
      }
      
      public function unfillSelectedSlot(qty:int) : void
      {
         this.unfillSlot(this._waitingSlot,qty);
      }
      
      public function fillDefaultSlot(item:Object, qty:int = -1) : void
      {
         var slot:Object = null;
         for each(slot in [this.slot_item,this.slot_rune,this.slot_sign])
         {
            if(this.dropValidatorFunction(slot,item,null))
            {
               if(qty == -1)
               {
                  switch(slot)
                  {
                     case this.slot_item:
                     case this.slot_sign:
                        qty = 1;
                        break;
                     case this.slot_rune:
                        qty = int(item.quantity);
                  }
               }
               this.fillSlot(slot,item,qty);
               return;
            }
         }
      }
      
      public function getMatchingSlot(item:Object) : Object
      {
         var slot:Object = null;
         for each(slot in [this.slot_item,this.slot_rune,this.slot_sign])
         {
            if(this.isValidSlot(slot,item))
            {
               return slot;
            }
         }
         return null;
      }
      
      public function getMatchingSlotFromUID(itemUID:int) : Object
      {
         var slot:Object = null;
         for each(slot in [this.slot_item,this.slot_rune,this.slot_sign])
         {
            if(Boolean(slot.data) && slot.data.objectUID == itemUID)
            {
               return slot;
            }
         }
         return null;
      }
      
      protected function getRunesFromInventory() : void
      {
         var value:int = 0;
         var resource:Object = null;
         var ei:EffectInstance = null;
         var valueBa:int = 0;
         var valuePa:int = 0;
         var valueRa:int = 0;
         var inventory:Object = this.storageApi.getViewContent("storageResources");
         for each(resource in inventory)
         {
            if(Boolean(resource) && resource.typeId == DataEnum.ITEM_TYPE_SMITHMAGIC_RUNE)
            {
               var _loc10_:int = 0;
               var _loc11_:* = resource.effects;
               for each(ei in _loc11_)
               {
                  if(!this._runesFromInventoryByEffectId[ei.effectId])
                  {
                     this._runesFromInventoryByEffectId[ei.effectId] = [{
                        "rune":null,
                        "fromBag":false
                     },{
                        "rune":null,
                        "fromBag":false
                     },{
                        "rune":null,
                        "fromBag":false
                     }];
                  }
                  valueBa = 1;
                  valuePa = 3;
                  valueRa = 10;
                  if(ei.effectId == ActionIds.ACTION_CHARACTER_BOOST_INITIATIVE || ei.effectId == ActionIds.ACTION_CHARACTER_BOOST_MAXIMUM_WEIGHT)
                  {
                     valueBa = 10;
                     valuePa = 30;
                     valueRa = 100;
                  }
                  else if(ei.effectId == ActionIds.ACTION_CHARACTER_BOOST_VITALITY)
                  {
                     valueBa = 5;
                     valuePa = 15;
                     valueRa = 50;
                  }
                  value = int(ei.parameter0);
                  if(value == valueBa)
                  {
                     this._runesFromInventoryByEffectId[ei.effectId][0].rune = resource;
                  }
                  else if(value == valuePa)
                  {
                     this._runesFromInventoryByEffectId[ei.effectId][1].rune = resource;
                  }
                  else if(value == valueRa)
                  {
                     this._runesFromInventoryByEffectId[ei.effectId][2].rune = resource;
                  }
               }
            }
         }
      }
      
      protected function getRunesByEffectId(id:int, index:int = -1) : Object
      {
         if(index == -1)
         {
            return this._runesFromInventoryByEffectId[id];
         }
         return this._runesFromInventoryByEffectId[id][index];
      }
      
      protected function getKnownRunes() : Array
      {
         return this._runesFromInventoryByEffectId;
      }
      
      protected function displayItem(itemW:Object, keepScrollbar:Boolean = false) : void
      {
         var o:Object = null;
         var delta:Object = null;
         var thEi:EffectInstance = null;
         var effectActionId:int = 0;
         var ei:EffectInstance = null;
         var e:Object = null;
         var i:* = 0;
         var item:Item = null;
         var eiZero:EffectInstance = null;
         var tempMin:int = 0;
         var index:int = 0;
         if(!itemW)
         {
            this.gd_itemEffects.dataProvider = new Array();
            this._itemToMage = itemW;
            this._smithForbidden = false;
            this.lbl_reliquat.text = "";
            this._reliquatObjetUid = -1;
            return;
         }
         this.afficherReliquat(itemW);
         if(!this._itemToMage || this._itemToMage.objectUID != itemW.objectUID)
         {
            this._newItem = true;
            this._effectsDeltas = null;
         }
         for each(thEi in itemW.effects)
         {
            if(Boolean(thEi) && thEi.effectId == ActionIds.PREVENT_FUTURE_SMITHMAGIC)
            {
               this._smithForbidden = true;
            }
         }
         this.btn_mergeAll.disabled = this.btn_mergeOnce.disabled = this._smithForbidden;
         if(!this._itemToMage || this._itemToMage.objectGID != itemW.objectGID)
         {
            item = this.dataApi.getItem(itemW.objectGID);
            this._itemEffects = new Array();
            this._idsTheoriques = new Array();
            for each(thEi in item.possibleEffects)
            {
               if(thEi)
               {
                  if(!(thEi.bonusType == 0 && thEi.category != 2))
                  {
                     if(thEi.bonusType == -1 && thEi.oppositeId != -1)
                     {
                        effectActionId = thEi.oppositeId;
                     }
                     else
                     {
                        effectActionId = int(thEi.effectId);
                     }
                     eiZero = this.dataApi.getNullEffectInstance(thEi);
                     o = {
                        "id":effectActionId,
                        "min":int.MIN_VALUE,
                        "max":int.MAX_VALUE,
                        "descZero":eiZero.description,
                        "effect":null,
                        "delta":0
                     };
                     if(thEi is EffectInstanceDice)
                     {
                        o.min = (thEi as EffectInstanceDice).diceNum;
                        if((thEi as EffectInstanceDice).diceSide == 0)
                        {
                           o.max = (thEi as EffectInstanceDice).diceNum;
                        }
                        else
                        {
                           o.max = (thEi as EffectInstanceDice).diceSide;
                        }
                     }
                     else if(thEi is EffectInstanceInteger)
                     {
                        o.min = (thEi as EffectInstanceInteger).value;
                        o.max = (thEi as EffectInstanceInteger).value;
                     }
                     else if(thEi is EffectInstanceMinMax)
                     {
                        o.min = (thEi as EffectInstanceMinMax).min;
                        o.max = (thEi as EffectInstanceMinMax).max;
                     }
                     if(thEi.bonusType < 0)
                     {
                        tempMin = int(o.min);
                        o.min = -1 * o.max;
                        o.max = -1 * tempMin;
                     }
                     this._itemEffects.push(o);
                     this._idsTheoriques.push(effectActionId);
                  }
               }
            }
         }
         var updatedEffectsId:Array = new Array();
         var existingEffectsId:Array = new Array();
         var allEffectsIdToCheckForDuplicateEffects:Array = new Array();
         for each(ei in itemW.effects)
         {
            if(ei.bonusType == -1 && ei.oppositeId != -1)
            {
               effectActionId = ei.oppositeId;
            }
            else
            {
               effectActionId = int(ei.effectId);
            }
            allEffectsIdToCheckForDuplicateEffects.push(effectActionId);
         }
         for each(e in this._itemEffects)
         {
            e.delta = 0;
            for each(ei in itemW.effects)
            {
               if(ei.bonusType == -1 && ei.oppositeId != -1)
               {
                  effectActionId = ei.oppositeId;
               }
               else
               {
                  effectActionId = int(ei.effectId);
               }
               if(Boolean(ei) && e.id == effectActionId)
               {
                  updatedEffectsId.push(e.id);
                  e.effect = ei;
                  if(this._effectsDeltas)
                  {
                     e.delta = 0;
                     for each(delta in this._effectsDeltas)
                     {
                        if(e.id == delta.id)
                        {
                           e.delta = delta.value;
                        }
                     }
                  }
               }
            }
            index = allEffectsIdToCheckForDuplicateEffects.indexOf(effectActionId);
            allEffectsIdToCheckForDuplicateEffects.splice(index,1);
         }
         for each(ei in itemW.effects)
         {
            if(ei.bonusType == -1 && ei.oppositeId != -1)
            {
               effectActionId = ei.oppositeId;
            }
            else
            {
               effectActionId = int(ei.effectId);
            }
            if(Boolean(ei) && updatedEffectsId.indexOf(effectActionId) == -1)
            {
               if(ei.bonusType == 0 && ei.category != 2)
               {
                  continue;
               }
               o = {
                  "id":effectActionId,
                  "min":int.MIN_VALUE,
                  "max":int.MAX_VALUE,
                  "descZero":"",
                  "effect":ei,
                  "delta":0
               };
               if(effectActionId == this._effectIdForRunesUpdate)
               {
                  this._runeTypeUpdate = -1;
               }
               if(this._effectsDeltas)
               {
                  for each(delta in this._effectsDeltas)
                  {
                     if(o.id == delta.id)
                     {
                        o.delta = delta.value;
                     }
                  }
               }
               else
               {
                  o.delta = 0;
               }
               this._itemEffects.push(o);
            }
            existingEffectsId.push(effectActionId);
         }
         i = 0;
         for(i = int(this._itemEffects.length - 1); i >= 0; i--)
         {
            if(Boolean(this._itemEffects[i]) && existingEffectsId.indexOf(this._itemEffects[i].id) == -1)
            {
               if(this._itemEffects[i].descZero == "")
               {
                  this._itemEffects.splice(i,1);
               }
               else
               {
                  this._itemEffects[i].effect = null;
                  if(this._effectsDeltas)
                  {
                     for each(delta in this._effectsDeltas)
                     {
                        if(this._itemEffects[i].id == delta.id)
                        {
                           this._itemEffects[i].delta = delta.value;
                        }
                     }
                  }
                  else
                  {
                     this._itemEffects[i].delta = 0;
                  }
               }
            }
         }
         this.gd_itemEffects.dataProvider = this._itemEffects;
         this._itemToMage = itemW;
      }
      
      protected function afficherReliquat(itemW:Object) : void
      {
         var ei:EffectInstance = null;
         var reliquat:int = 0;
         for each(ei in itemW.effects)
         {
            if(Boolean(ei) && ei.effectId == 351 && ei is EffectInstanceInteger)
            {
               reliquat = int(EffectInstanceInteger(ei).value);
            }
         }
         if(this._reliquatObjetUid != itemW.objectUID)
         {
            this._reliquatDelta = 0;
         }
         else if(this._resultatFmAttendu)
         {
            this._reliquatDelta = reliquat - this._reliquatValeur;
         }
         this._resultatFmAttendu = false;
         this._reliquatObjetUid = itemW.objectUID;
         this._reliquatValeur = reliquat;
         var texte:String = "Reliquat : " + reliquat;
         if(this._reliquatDelta > 0)
         {
            texte += " (+" + this._reliquatDelta + ")";
         }
         else if(this._reliquatDelta < 0)
         {
            texte += " (" + this._reliquatDelta + ")";
         }
         this.lbl_reliquat.text = texte;
      }

      protected function estRuneDeLigne(target:Object) : Boolean
      {
         return Boolean(target) && target != this.slot_rune && target is Slot && Boolean(target.name) && String(target.name).indexOf("slot_rune") == 0;
      }

      protected function fusionAvancee(rune:Object) : void
      {
         if(!rune || !this.slot_item.data || this._smithForbidden || this._multiCraft || !this.isValidSlot(this.slot_rune,rune))
         {
            return;
         }
         if(Boolean(this.slot_rune.data) && this.slot_rune.data.objectGID == rune.objectGID)
         {
            if(!this.btn_mergeOnce.disabled)
            {
               this.onRelease(this.btn_mergeOnce);
            }
            return;
         }
         this._fusionAvanceeGid = rune.objectGID;
         this.fillSlot(this.slot_rune,rune,int(rune.quantity));
      }

      protected function viderHistorique() : void
      {
         this.texta_log.htmlText = "";
         while(Job.getInstance().mageLog.length > 0)
         {
            Job.getInstance().removeMageLogFirstLine();
         }
      }

      protected function dropValidatorFunction(target:Object, data:Object, source:Object) : Boolean
      {
         return this.isValidSlot(target,data);
      }
      
      protected function isValidSlot(target:Object, d:Object) : Boolean
      {
         if(!this._skill)
         {
            return false;
         }
         switch(target)
         {
            case this.slot_item:
               if(this._skill.modifiableItemTypeIds.indexOf(d.typeId) == -1)
               {
                  return false;
               }
               return true;
               break;
            case this.slot_rune:
               if((!this._skill.isForgemagus || this._runesItemTypes.indexOf(d.typeId) == -1) && d.typeId != DataEnum.ITEM_TYPE_SMITHMAGIC_POTION && d.typeId != DataEnum.ITEM_TYPE_SMITHMAGIC_ORB)
               {
                  return false;
               }
               return true;
               break;
            case this.slot_sign:
               if(d.objectGID != DataEnum.ITEM_GID_SIGNATURE_RUNE)
               {
                  return false;
               }
               return true;
               break;
            default:
               return false;
         }
      }
      
      protected function processDropFunction(target:Object, d:Object, source:Object) : void
      {
         if(this.dropValidatorFunction(target,d,source))
         {
            switch(target)
            {
               case this.slot_item:
               case this.slot_sign:
                  this.fillSlot(target,d,1);
                  break;
               case this.slot_rune:
                  if(d.info1 > 1)
                  {
                     this._waitingObject = d;
                     this.modCommon.openQuantityPopup(1,d.quantity,d.quantity,this.onValidQtyDropToSlot);
                  }
                  else
                  {
                     this.fillSlot(this.slot_rune,d,1);
                  }
            }
         }
      }
      
      protected function fillSlot(slot:Object, item:Object, qty:int) : void
      {
         if(slot.data != null && (slot == this.slot_item || slot == this.slot_sign || slot == this.slot_rune && slot.data.objectGID != item.objectGID))
         {
            this.unfillSlot(slot,-1);
            if(slot == this.slot_rune)
            {
               this._corruptionOrTranscendanceRuneUsed = false;
            }
            this._refill_item = item;
            this._refill_qty = qty;
         }
         else
         {
            if(Boolean(slot) && Boolean(slot == this.slot_rune) && (item.typeId == DataEnum.ITEM_TYPE_SMITHMAGIC_CORRUPTION_RUNE || item.typeId == DataEnum.ITEM_TYPE_SMITHMAGIC_TRANSCENDANCE_RUNE))
            {
               this._corruptionOrTranscendanceRuneUsed = true;
            }
            else if(Boolean(slot) && Boolean(slot == this.slot_rune) && (item.typeId != DataEnum.ITEM_TYPE_SMITHMAGIC_CORRUPTION_RUNE && item.typeId != DataEnum.ITEM_TYPE_SMITHMAGIC_TRANSCENDANCE_RUNE))
            {
               this._corruptionOrTranscendanceRuneUsed = false;
            }
            this._moveRequestedItemUid = item.objectUID;
            this.sysApi.sendAction(new ExchangeObjectMove(item.objectUID,qty));
         }
      }
      
      protected function unfillSlot(slot:Object, qty:int = -1) : void
      {
         if(qty == -1)
         {
            qty = int(slot.data.quantity);
         }
         if(slot == this.slot_rune)
         {
            this._corruptionOrTranscendanceRuneUsed = false;
         }
         this._moveRequestedItemUid = slot.data.objectUID;
         this.sysApi.sendAction(new ExchangeObjectMove(slot.data.objectUID,-qty));
      }
      
      protected function setMergeButtonDisabled(disabled:Boolean) : void
      {
         if(this._mergeButtonTimer)
         {
            this._mergeButtonTimer.stop();
         }
         if(!disabled && (!this.slot_item.data || !this.slot_rune.data) && !this._smithForbidden)
         {
            disabled = true;
         }
         this.btn_mergeOnce.disabled = disabled;
         if(this._multiCraft)
         {
            this.btn_lbl_btn_mergeAll.text = this.uiApi.getText("ui.common.stop");
            this.btn_mergeAll.disabled = false;
         }
         else
         {
            this.btn_lbl_btn_mergeAll.text = this.uiApi.getText("ui.common.mergeAll");
            this.btn_mergeAll.disabled = disabled;
         }
      }
      
      protected function addLogLine(text:String, cssClass:String) : void
      {
         var idel:uint = 0;
         var htmlText:String = null;
         var index:uint = 0;
         this.texta_log.appendText(text,cssClass);
         Job.getInstance().addToMageLog({
            "text":text,
            "cssClass":cssClass
         });
         if(Job.getInstance().mageLog.length > 100)
         {
            htmlText = this.texta_log.htmlText;
            index = htmlText.indexOf("</TEXTFORMAT>") + 13;
            this.texta_log.htmlText = htmlText.substr(index);
            Job.getInstance().removeMageLogFirstLine();
         }
      }
      
      protected function pictoNameFromSkillId(skillId:int) : String
      {
         switch(skillId)
         {
            case DataEnum.SKILL_MAGE_JEWEL:
               return "amulet_slot_uri";
            case DataEnum.SKILL_MAGE_SHOES:
               return "boots_slot_uri";
            case DataEnum.SKILL_MAGE_TAILOR:
               return "helmet_slot_uri";
            default:
               return "weapon_slot_uri";
         }
      }
      
      public function onRelease(target:Object) : void
      {
         switch(target)
         {
            case this.btn_mergeAll:
               if(!this._corruptionOrTranscendanceRuneUsed)
               {
                  this._multiCraft = !this._multiCraft;
                  if(this._multiCraft)
                  {
                     this.setMergeButtonDisabled(true);
                     this.sysApi.sendAction(new ExchangeReplay(-1));
                     this.sysApi.sendAction(new ExchangeReady(true));
                  }
                  else
                  {
                     this.sysApi.sendAction(new ExchangeReplayStop());
                  }
               }
               else if(this._popupName == null)
               {
                  this.setMergeButtonDisabled(true);
                  this._popupName = this.modCommon.openPopup(this.uiApi.getText("ui.popup.warning"),this.uiApi.getText("ui.smithmagic.futureUpgradesImpossible"),[this.uiApi.getText("ui.common.yes"),this.uiApi.getText("ui.common.cancel")],[this.sendSmithRequest,this.cancelSmithRequest],this.sendSmithRequest,this.cancelSmithRequest);
               }
               break;
            case this.btn_mergeOnce:
               this._multiCraft = false;
               this.setMergeButtonDisabled(true);
               if(!this._corruptionOrTranscendanceRuneUsed)
               {
                  this._mergeButtonTimer.start();
                  this._mergeButtonTimerOut = false;
                  this._mergeResultGot = false;
                  this.sysApi.sendAction(new ExchangeReady(true));
               }
               else if(this._popupName == null)
               {
                  this._popupName = this.modCommon.openPopup(this.uiApi.getText("ui.popup.warning"),this.uiApi.getText("ui.smithmagic.futureUpgradesImpossible"),[this.uiApi.getText("ui.common.yes"),this.uiApi.getText("ui.common.cancel")],[this.sendSmithRequest,this.cancelSmithRequest],this.sendSmithRequest,this.cancelSmithRequest);
               }
               break;
            case this.btn_close:
               this.uiApi.unloadUi(this.uiApi.me().name);
               break;
            case this.ed_rightCharacter:
               this.sysApi.sendAction(new DisplayContextualMenu(this.playerApi.id()));
               break;
            case this.btn_modeAvance:
               this._modeAvance = this.btn_modeAvance.selected;
               this.sysApi.setData("mystic_fm_mode_avance",this._modeAvance);
               break;
            case this.btn_viderHistorique:
               this.viderHistorique();
               break;
            default:
               if(this._modeAvance && this.estRuneDeLigne(target) && Boolean(target.data))
               {
                  this.fusionAvancee(target.data);
               }
         }
      }
      
      public function sendSmithRequest() : void
      {
         this._popupName = null;
         this.sysApi.sendAction(new ExchangeReady(true));
      }
      
      public function cancelSmithRequest() : void
      {
         this._popupName = null;
         this.setMergeButtonDisabled(false);
      }
      
      public function onDoubleClick(target:Object) : void
      {
         if(Boolean(target.data) && Boolean(!this.uiApi.keyIsDown(Keyboard.CONTROL)) && !this.uiApi.keyIsDown(15))
         {
            if(target == this.slot_item || target == this.slot_rune || target == this.slot_sign)
            {
               this.unfillSlot(target,1);
            }
            else
            {
               this.sysApi.dispatchHook(DoubleClickItemInventory,target.data,1);
            }
         }
      }
      
      public function onMouseCtrlDoubleClick(target:Object) : void
      {
         var iw:ItemWrapper = null;
         var alreadyInSlotQty:uint = 0;
         if(Boolean(target.data) && (this.uiApi.keyIsDown(Keyboard.CONTROL) || this.uiApi.keyIsDown(15)))
         {
            if(target == this.slot_item || target == this.slot_rune || target == this.slot_sign)
            {
               this.unfillSlot(target,-1);
            }
            else
            {
               iw = this.inventoryApi.getItem(target.data.objectUID);
               if(iw)
               {
                  alreadyInSlotQty = 0;
                  if(Boolean(this.slot_rune.data) && Boolean(this.isValidSlot(this.slot_rune,iw)) && this.slot_rune.data.objectGID == iw.objectGID)
                  {
                     alreadyInSlotQty = uint(this.slot_rune.data.quantity);
                  }
                  this.sysApi.dispatchHook(DoubleClickItemInventory,target.data,iw.quantity - alreadyInSlotQty);
               }
               else
               {
                  this.sysApi.dispatchHook(DoubleClickItemInventory,target.data,target.data.quantity);
               }
            }
         }
      }
      
      public function onMouseAltDoubleClick(target:Object) : void
      {
         var slotClicked:Boolean = false;
         if(target.data)
         {
            slotClicked = false;
            if(target == this.slot_item || target == this.slot_rune || target == this.slot_sign)
            {
               slotClicked = true;
            }
            if(!slotClicked)
            {
               return;
            }
            this._altClickedSlot = target;
            this.modCommon.openQuantityPopup(1,target.data.quantity,target.data.quantity,this.onValidQty);
         }
      }
      
      public function onRollOut(target:Object) : void
      {
         this.uiApi.hideTooltip();
      }
      
      public function onRollOver(target:Object) : void
      {
         var itemTypeId:int = 0;
         var itemsText:String = "";
         if(target.data)
         {
            this.uiApi.showTooltip(target.data,target,false,"standard",0,0,0,"itemName",null,{
               "showEffects":true,
               "header":true
            },"ItemInfo");
            return;
         }
         if(target == this.slot_item)
         {
            for each(itemTypeId in this._skill.modifiableItemTypeIds)
            {
               itemsText += this.dataApi.getItemType(itemTypeId).name + "/";
            }
            itemsText = itemsText.substr(0,itemsText.length - 1);
         }
         else if(target == this.slot_rune)
         {
            itemsText = this.dataApi.getItemType(78).name;
         }
         else if(target == this.slot_sign)
         {
            itemsText = this._skillLevel < ProtocolConstantsEnum.MAX_JOB_LEVEL ? this.uiApi.getText("ui.craft.jobLevelLowForSignatureClient") : this.dataApi.getItem(7508).name;
         }
         this.uiApi.showTooltip(this.uiApi.textTooltipInfo(itemsText),target,false,"standard",1,7,3,null,null,null,"TextInfo");
      }
      
      public function onRightClick(target:Object) : void
      {
         var data:Object = null;
         var contextMenu:Object = null;
         if(target == this.ed_rightCharacter)
         {
            this.sysApi.sendAction(new DisplayContextualMenu(this.playerApi.id()));
         }
         else if(target.data)
         {
            data = target.data;
            contextMenu = this.menuApi.create(data);
            if(contextMenu.content.length > 0)
            {
               this.modContextMenu.createContextMenu(contextMenu);
            }
         }
      }
      
      private function onDropStart(src:Object) : void
      {
         var slot:Object = null;
         this._waitingSlot = src;
         for each(slot in [this.slot_item,this.slot_rune,this.slot_sign])
         {
            if(this.dropValidatorFunction(slot,src.data,null))
            {
               slot.selected = true;
            }
         }
      }
      
      private function onDropEnd(src:Object) : void
      {
         var slot:Object = null;
         for each(slot in [this.slot_item,this.slot_rune,this.slot_sign])
         {
            slot.selected = false;
         }
      }
      
      protected function onExchangeObjectModified(item:Object, remote:Object) : void
      {
         var slot:Object = this.getMatchingSlot(item);
         this.storageApi.addItemMask(item.objectUID,"smithMagic",item.quantity);
         this.storageApi.releaseHooks();
         var oldQuantity:int = slot.data ? int(slot.data.quantity) : 0;
         slot.data = item;
         switch(slot)
         {
            case this.slot_item:
               this.displayItem(item,true);
         }
         this._moveRequestedItemUid = 0;
      }
      
      protected function onExchangeObjectAdded(item:Object, remote:Object) : void
      {
         var slot:Object = this.getMatchingSlot(item);
         if(Boolean(slot.data) && slot.data.objectUID != item.objectUID)
         {
            this.storageApi.removeItemMask(slot.data.objectUID,"smithMagic");
         }
         slot.colorTransform = DEFAULT_CT;
         slot.iconColorTransform = DEFAULT_CT;
         slot.data = item;
         this.storageApi.addItemMask(item.objectUID,"smithMagic",item.quantity);
         this.storageApi.releaseHooks();
         this.soundApi.playSound(SoundTypeEnum.SWITCH_RIGHT_TO_LEFT);
         switch(slot)
         {
            case this.slot_item:
               this.displayItem(item,false);
         }
         if(Boolean(this.slot_item.data) && Boolean(this.slot_rune.data) && !this._smithForbidden)
         {
            this.setMergeButtonDisabled(false);
            if(this._fusionAvanceeGid != 0 && slot == this.slot_rune && item.objectGID == this._fusionAvanceeGid)
            {
               this._fusionAvanceeGid = 0;
               this.onRelease(this.btn_mergeOnce);
            }
         }
         this._moveRequestedItemUid = 0;
      }
      
      protected function onExchangeObjectRemoved(itemUID:int, remote:Boolean) : void
      {
         this.storageApi.removeItemMask(itemUID,"smithMagic");
         this.storageApi.releaseHooks();
         var slot:Object = this.getMatchingSlotFromUID(itemUID);
         if(slot)
         {
            slot.iconColorTransform = ICON_CT;
            slot.data = null;
            if(slot == this.slot_item)
            {
               this.displayItem(null,false);
            }
            this.soundApi.playSound(SoundTypeEnum.SWITCH_LEFT_TO_RIGHT);
            if(this._refill_item != null)
            {
               this.fillSlot(slot,this._refill_item,this._refill_qty);
               this._refill_item = null;
            }
            if(this.slot_item.data == null || this.slot_rune.data == null)
            {
               this.setMergeButtonDisabled(true);
            }
         }
         this._moveRequestedItemUid = 0;
      }
      
      protected function onObjectDeleted(item:Object) : void
      {
         var knownRunes:Object = null;
         var effectStr:String = null;
         var effectId:int = 0;
         if(Boolean(item) && Boolean(this._runesItemTypes.indexOf(item.typeId) != -1) && item.objectUID != this._moveRequestedItemUid)
         {
            knownRunes = this.getKnownRunes();
            for(effectStr in knownRunes)
            {
               effectId = int(effectStr);
               if(Boolean(this.getRunesByEffectId(effectId,0)) && Boolean(this.getRunesByEffectId(effectId,0).rune) && this.getRunesByEffectId(effectId,0).rune.objectUID == item.objectUID)
               {
                  this.getRunesByEffectId(effectId,0).rune = null;
                  this._effectIdForRunesUpdate = effectId;
                  this._runeTypeUpdate = 0;
                  break;
               }
               if(Boolean(this.getRunesByEffectId(effectId,1)) && Boolean(this.getRunesByEffectId(effectId,1).rune) && this.getRunesByEffectId(effectId,1).rune.objectUID == item.objectUID)
               {
                  this.getRunesByEffectId(effectId,1).rune = null;
                  this._effectIdForRunesUpdate = int(effectId);
                  this._runeTypeUpdate = 1;
                  break;
               }
               if(Boolean(this.getRunesByEffectId(effectId,2)) && Boolean(this.getRunesByEffectId(effectId,2).rune) && this.getRunesByEffectId(effectId,2).rune.objectUID == item.objectUID)
               {
                  this.getRunesByEffectId(effectId,2).rune = null;
                  this._effectIdForRunesUpdate = int(effectId);
                  this._runeTypeUpdate = 2;
                  break;
               }
            }
         }
      }
      
      protected function onObjectQuantity(item:ItemWrapper, quantity:int, oldQuantity:int) : void
      {
         var ei:EffectInstance = null;
         if(Boolean(item) && Boolean(this._runesItemTypes.indexOf(item.typeId) != -1) && item.objectUID != this._moveRequestedItemUid)
         {
            for each(ei in item.effects)
            {
               if(this.getRunesByEffectId(ei.effectId))
               {
                  if(Boolean(this.getRunesByEffectId(ei.effectId,0)) && Boolean(this.getRunesByEffectId(ei.effectId,0).rune != null) && this.getRunesByEffectId(ei.effectId,0).rune.objectUID == item.objectUID)
                  {
                     this.getRunesByEffectId(ei.effectId,0).rune = item;
                     this._effectIdForRunesUpdate = ei.effectId;
                     this._runeTypeUpdate = 0;
                     return;
                  }
                  if(Boolean(this.getRunesByEffectId(ei.effectId,1)) && Boolean(this.getRunesByEffectId(ei.effectId,1).rune != null) && this.getRunesByEffectId(ei.effectId,1).rune.objectUID == item.objectUID)
                  {
                     this.getRunesByEffectId(ei.effectId,1).rune = item;
                     this._effectIdForRunesUpdate = ei.effectId;
                     this._runeTypeUpdate = 1;
                     return;
                  }
                  if(Boolean(this.getRunesByEffectId(ei.effectId,2)) && Boolean(this.getRunesByEffectId(ei.effectId,2).rune != null) && this.getRunesByEffectId(ei.effectId,2).rune.objectUID == item.objectUID)
                  {
                     this.getRunesByEffectId(ei.effectId,2).rune = item;
                     this._effectIdForRunesUpdate = ei.effectId;
                     this._runeTypeUpdate = 2;
                     return;
                  }
               }
            }
         }
      }
      
      public function onExchangeCraftResult(result:int, item:Object) : void
      {
         switch(result)
         {
            case CRAFT_IMPOSSIBLE:
               this.modCommon.openPopup(this.uiApi.getText("ui.common.error"),this.uiApi.getText("ui.craft.noResult"),[this.uiApi.getText("ui.common.ok")]);
               this.sysApi.sendAction(new ExchangeReady(false));
            case CRAFT_FAILED:
            case CRAFT_SUCCESS:
            case CRAFT_NEARLY_SUCCESS:
               if(item)
               {
                  this._resultatFmAttendu = true;
                  if(this.slot_item.data)
                  {
                     this.slot_item.data = this.dataApi.getItemWrapper(item.objectGID,item.position,this.slot_item.data.objectUID,item.quantity,item.effectsList);
                  }
                  this.displayItem(item,true);
               }
         }
         if(!this._multiCraft)
         {
            this._mergeResultGot = true;
            if(this._mergeButtonTimerOut)
            {
               this.setMergeButtonDisabled(false);
            }
         }
      }
      
      public function onMergeButtonTimer(e:TimerEvent) : void
      {
         this._mergeButtonTimerOut = true;
         if(this._mergeResultGot)
         {
            this.setMergeButtonDisabled(false);
         }
      }
      
      public function processDropToInventory(target:Object, d:Object, source:Object) : void
      {
         if(d.info1 > 1)
         {
            this._waitingObject = d;
            this.modCommon.openQuantityPopup(1,d.quantity,d.quantity,this.onValidQtyDropToInventory);
         }
         else
         {
            this.unfillSelectedSlot(1);
         }
      }
      
      public function onDoubleClickItemInventory(item:Object, qty:int = 1) : void
      {
         if(item.id == DataEnum.ITEM_GID_SIGNATURE_RUNE || this._runesItemTypes.indexOf(item.id) != -1)
         {
            qty = 1;
         }
         this.fillDefaultSlot(item,qty);
      }
      
      private function onJobLevelUp(jobId:uint, jobName:String, newLevel:uint) : void
      {
         if(jobId == this._job.id)
         {
            this.lbl_job.text = this._job.name + " " + this.uiApi.getText("ui.common.short.level") + " " + newLevel;
         }
      }
      
      public function onExchangeItemAutoCraftStoped(reason:int) : void
      {
         this.btn_mergeAll.soundId = SoundEnum.OK_BUTTON;
         this._multiCraft = false;
         this.setMergeButtonDisabled(false);
      }
      
      public function onItemMagedResult(success:Boolean, result:String, deltas:Object) : void
      {
         var style:String = "normal";
         if(this._newItem && Boolean(this._itemToMage))
         {
            this.addLogLine(this._itemToMage.name,style);
            this._newItem = false;
         }
         var d:Object = null;
         var exo:Boolean = false;
         var perte:Boolean = false;
         if(success)
         {
            for each(d in deltas)
            {
               if(Boolean(d) && d.value > 0 && this._idsTheoriques.indexOf(d.id) == -1)
               {
                  exo = true;
               }
               if(Boolean(d) && d.value < 0)
               {
                  perte = true;
               }
            }
            style = exo ? "exo" : (perte ? "neutre" : "success");
         }
         else
         {
            style = "fail";
         }
         this.addLogLine(result,style);
         this.texta_log.scrollV = this.texta_log.maxScrollV;
         this._effectsDeltas = deltas;
      }
      
      public function onExchangeLeave(sucess:Boolean) : void
      {
         this.uiApi.unloadUi(this.uiApi.me().name);
      }
      
      public function getIsAvailableItem(item:Object) : Boolean
      {
         if(!this.dataApi)
         {
            return true;
         }
         return this.getMatchingSlot(item) != null;
      }
      
      private function onValidQty(qty:Number) : void
      {
         this.unfillSlot(this._altClickedSlot,qty);
      }
      
      private function onValidQtyDropToSlot(qty:Number) : void
      {
         this.fillDefaultSlot(this._waitingObject,qty);
      }
      
      protected function onValidQtyDropToInventory(qty:Number) : void
      {
         this.unfillSelectedSlot(qty);
      }
   }
}

