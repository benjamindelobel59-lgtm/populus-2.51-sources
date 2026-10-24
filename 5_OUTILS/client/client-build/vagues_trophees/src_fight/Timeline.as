package ui
{
   import com.ankamagames.berilia.api.UiApi;
   import com.ankamagames.berilia.components.Label;
   import com.ankamagames.berilia.components.Texture;
   import com.ankamagames.berilia.types.graphic.ButtonContainer;
   import com.ankamagames.berilia.types.graphic.GraphicContainer;
   import com.ankamagames.berilia.types.tooltip.TooltipRectangle;
   import com.ankamagames.dofus.internalDatacenter.fight.FighterInformations;
   import com.ankamagames.dofus.uiApi.FightApi;
   import com.ankamagames.dofus.uiApi.PlayedCharacterApi;
   import com.ankamagames.dofus.uiApi.SystemApi;
   import com.ankamagames.dofus.uiApi.TooltipApi;
   import d2actions.*;
   import d2enums.ComponentHookList;
   import d2enums.TeamEnum;
   import d2hooks.BuffAdd;
   import d2hooks.BuffDispell;
   import d2hooks.BuffRemove;
   import d2hooks.BuffUpdate;
   import d2hooks.FightEvent;
   import d2hooks.FighterInfoUpdate;
   import d2hooks.FighterSelected;
   import d2hooks.FightersListUpdated;
   import d2hooks.FoldAll;
   import d2hooks.GameFightPause;
   import d2hooks.GameFightTurnEnd;
   import d2hooks.GameFightTurnStart;
   import d2hooks.HideDeadFighters;
   import d2hooks.HideSummonedFighters;
   import d2hooks.OrderFightersSwitched;
   import d2hooks.TurnCountUpdated;
   import d2hooks.UpdateFightersLook;
   import d2hooks.UpdatePreFightersList;
   import d2hooks.WaveUpdated;
   import flash.display.Sprite;
   import flash.filters.GlowFilter;
   import flash.geom.Point;
   import flash.utils.Dictionary;
   import ui.timeline.Fighter;
   
   public class Timeline
   {
      
      public var sysApi:SystemApi;
      
      public var uiApi:UiApi;
      
      public var fightApi:FightApi;
      
      public var playerApi:PlayedCharacterApi;
      
      public var tooltipApi:TooltipApi;
      
      private var _fighters:Array;
      
      private var _hiddenFighters:Array;
      
      private var _fightersId:Object;
      
      private var _unsortedFightersId:Object;
      
      private var _hideDeadGuys:Boolean = false;
      
      private var _hideSummonedStuff:Boolean = false;
      
      private var _currentFighterWaitTime:int;
      
      private var _currentFighterRemainingTime:uint;
      
      private var _selectedFighter:Fighter = null;
      
      private var _currentFighter:Fighter = null;
      
      private var _timelineMask:Sprite;
      
      private var _screenWidth:int;
      
      private var _charCtrOffset:int;
      
      private var _turnCount:uint = 1;
      
      private var _waveCountAttackers:uint = 0;
      
      private var _turnCountBeforeNextWaveAttackers:int = -1;
      
      private var _waveCountDefenders:uint = 0;
      
      private var _turnCountBeforeNextWaveDefenders:int = -1;

      private var _waveTotalAttackers:int = 0;

      private var _waveTotalDefenders:int = 0;
      
      private var _rollOverMe:Boolean = false;
      
      private var _prefightPhase:Boolean = true;
      
      public var timelineCtr:GraphicContainer;
      
      public var charCtr:GraphicContainer;
      
      public var charFrames:GraphicContainer;
      
      public var ctr_timeline:GraphicContainer;
      
      public var ctr_buffs:GraphicContainer;
      
      public var lbl_turnCount:Label;
      
      public var btn_minimArrow:ButtonContainer;
      
      public var btn_rightArrow:ButtonContainer;
      
      public var btn_leftArrow:ButtonContainer;
      
      public var tx_currentArrow:Texture;
      
      public var ctr_background:GraphicContainer;
      
      public var ctr_inviBounds:GraphicContainer;
      
      public var tx_wave:Texture;
      
      public function Timeline()
      {
         super();
      }
      
      public function main(turnList:Object) : void
      {
         this.sysApi.addHook(GameFightTurnEnd,this.onGameFightTurnEnd);
         this.sysApi.addHook(FightersListUpdated,this.onGameFightTurnListUpdated);
         this.sysApi.addHook(UpdatePreFightersList,this.onUpdatePreFightersList);
         this.sysApi.addHook(UpdateFightersLook,this.onUpdateFightersLook);
         this.sysApi.addHook(GameFightTurnStart,this.onGameFightTurnStart);
         this.sysApi.addHook(FightEvent,this.onFightEvent);
         this.sysApi.addHook(FoldAll,this.onFoldAll);
         this.sysApi.addHook(FighterSelected,this.onFighterSelected);
         this.sysApi.addHook(BuffAdd,this.onBuffAdd);
         this.sysApi.addHook(BuffDispell,this.onBuffDispell);
         this.sysApi.addHook(BuffRemove,this.onBuffRemove);
         this.sysApi.addHook(BuffUpdate,this.onBuffUpdate);
         this.sysApi.addHook(TurnCountUpdated,this.onTurnCountUpdated);
         this.sysApi.addHook(OrderFightersSwitched,this.onOrderFightersSwitched);
         this.sysApi.addHook(HideDeadFighters,this.onHideDeadFighters);
         this.sysApi.addHook(HideSummonedFighters,this.onHideSummonedFighters);
         this.sysApi.addHook(WaveUpdated,this.onWaveUpdated);
         this.sysApi.addHook(GameFightPause,this.onGameFightPause);
         this.sysApi.addHook(FighterInfoUpdate,this.onFighterMouseAction);
         this.uiApi.addComponentHook(this.lbl_turnCount,ComponentHookList.ON_ROLL_OVER);
         this.uiApi.addComponentHook(this.lbl_turnCount,ComponentHookList.ON_ROLL_OUT);
         this.uiApi.addComponentHook(this.tx_wave,ComponentHookList.ON_ROLL_OVER);
         this.uiApi.addComponentHook(this.tx_wave,ComponentHookList.ON_ROLL_OUT);
         this._fighters = new Array();
         this._hiddenFighters = new Array();
         this.tx_currentArrow.visible = false;
         var glow:GlowFilter = new GlowFilter(Api.sysApi.getConfigEntry("colors.text.glow.dark"),0.8,2,2,6,3);
         this.lbl_turnCount.filters = [glow];
         this._screenWidth = this.uiApi.getStageWidth();
         this._charCtrOffset = int(this.uiApi.me().getConstant("char_ctr_offset"));
         this._hideDeadGuys = this.sysApi.getOption("hideDeadFighters","dofus");
         this._hideSummonedStuff = this.sysApi.getOption("hideSummonedFighters","dofus");
         this._timelineMask = new Sprite();
         this._timelineMask.graphics.beginFill(16733440);
         this._timelineMask.graphics.drawRect(-this._screenWidth + 170,0,this._screenWidth - 110,this.timelineCtr.height);
         this._timelineMask.graphics.endFill();
         this.timelineCtr.addChild(this._timelineMask);
         this._timelineMask.visible = false;
         this.refreshTimeline();
      }
      
      public function unload() : void
      {
         var fighter:Fighter = null;
         Fighter.cleanFramesPool();
         for(var i:int = 0; i < this._fighters.length; i++)
         {
            fighter = this._fighters[i];
            fighter.destroy();
         }
         this._fighters = null;
         this._hiddenFighters = null;
      }
      
      public function set folded(value:Boolean) : void
      {
         var te:Boolean = this.isTimelineExtended();
         this.btn_leftArrow.visible = !value && te;
         this.btn_rightArrow.visible = !value && te;
         this.charFrames.visible = !value;
         if(!this._prefightPhase)
         {
            this.tx_currentArrow.visible = !value;
         }
         this.ctr_background.visible = !value;
         this.lbl_turnCount.visible = !value;
         this.tx_wave.visible = !value && (this._waveCountAttackers > 0 || this._waveCountDefenders > 0);
         this.fightApi.setSwapPositionRequestsIconsVisibility(!value);
      }
      
      public function get folded() : Boolean
      {
         return !this.charFrames.visible;
      }
      
      public function moveLeft() : void
      {
         var offset:int = this.charCtr.contentWidth - this._timelineMask.width - this._charCtrOffset;
         if(this.charFrames.x + this._charCtrOffset + 300 < this._timelineMask.x + offset)
         {
            this.charFrames.x += 300;
         }
         else
         {
            this.charFrames.x = this._timelineMask.x + offset;
         }
         this.updateTurnOfArrow();
      }
      
      public function moveRight() : void
      {
         if(this.charFrames.x + this._charCtrOffset > this._timelineMask.x + 300)
         {
            this.charFrames.x -= 300;
         }
         else
         {
            this.charFrames.x = this._timelineMask.x;
         }
         this.updateTurnOfArrow();
      }
      
      public function refreshMoveOffset() : void
      {
         if(this.charCtr.contentWidth <= this._timelineMask.width)
         {
            this.charFrames.x = -this.charFrames.width + this._charCtrOffset;
         }
         else if(this.charFrames.x + this._charCtrOffset < this._timelineMask.x)
         {
            this.charFrames.x = this._timelineMask.x;
         }
         else if(this.charFrames.x + this._charCtrOffset - this.charCtr.contentWidth > this._timelineMask.x - this._timelineMask.width)
         {
            this.charFrames.x = this._timelineMask.x - this._timelineMask.width + this.charCtr.contentWidth;
         }
      }
      
      private function createFighter(id:Number, num:uint) : Fighter
      {
         var f:Fighter = new Fighter(id,this.uiApi.me(),num);
         this.uiApi.addComponentHook(f.frame,"onRelease");
         this.uiApi.addComponentHook(f.frame,"onRollOver");
         this.uiApi.addComponentHook(f.frame,"onRollOut");
         return f;
      }
      
      private function refreshTimeline() : void
      {
         var fighter:Fighter = null;
         var oldfighter:Fighter = null;
         var id:Number = NaN;
         var pos:* = 0;
         var infos:FighterInformations = null;
         var alive:Boolean = false;
         var deadId:Number = NaN;
         var isInPreFight:Boolean = this.playerApi.isInPreFight();
         var oldFighters:Dictionary = new Dictionary();
         for each(oldfighter in this._fighters)
         {
            oldFighters[oldfighter.id] = oldfighter;
         }
         this._fightersId = this.fightApi.getFighters();
         if(!this._fightersId)
         {
            this._fightersId = new Array();
         }
         var deadFightersId:Object = this.fightApi.getDeadFighters();
         if(!deadFightersId)
         {
            deadFightersId = new Array();
         }
         this._fighters = new Array();
         this._hiddenFighters = new Array();
         var numFighter:uint = this._fightersId.length - deadFightersId.length;
         if(isInPreFight)
         {
            for(pos = int(this._fightersId.length - 1); pos >= 0; pos--)
            {
               id = Number(this._fightersId[pos]);
               infos = Api.fightApi.getFighterInformations(id);
               if(infos.hiddenInPrefight)
               {
                  numFighter--;
               }
            }
         }
         for(pos = int(this._fightersId.length - 1); pos >= 0; pos--)
         {
            id = Number(this._fightersId[pos]);
            infos = Api.fightApi.getFighterInformations(id);
            alive = true;
            for each(deadId in deadFightersId)
            {
               if(deadId == id)
               {
                  alive = false;
                  break;
               }
            }
            if(oldFighters[id])
            {
               if((!this._hideDeadGuys || alive) && (!this._hideSummonedStuff || !oldFighters[id].summoned))
               {
                  fighter = oldFighters[id];
                  fighter.alive = alive;
                  fighter.hiddenInPrefight = infos.hiddenInPrefight;
                  fighter.updateNumber(numFighter);
                  if(!fighter.alive || fighter.hiddenInPrefight && isInPreFight)
                  {
                     numFighter++;
                  }
                  oldFighters[id] = null;
                  this._fighters.push(fighter);
               }
               else
               {
                  this._hiddenFighters[id] = {
                     "id":id,
                     "alive":alive,
                     "summoned":oldFighters[id].summoned
                  };
                  this.charCtr.removeChild(oldFighters[id].frame);
                  if(!alive || Boolean(oldFighters[id].hiddenInPrefight) && Boolean(isInPreFight))
                  {
                     numFighter++;
                  }
                  oldFighters[id].destroy(true);
                  fighter = null;
               }
            }
            else
            {
               infos = Api.fightApi.getFighterInformations(id);
               if(Boolean(infos) && (Boolean(!this._hideDeadGuys || alive)) && (!this._hideSummonedStuff || !infos.summoned))
               {
                  fighter = this.createFighter(id,numFighter);
                  fighter.alive = alive;
                  fighter.hiddenInPrefight = infos.hiddenInPrefight;
                  if(!fighter.alive || fighter.hiddenInPrefight && isInPreFight)
                  {
                     numFighter++;
                  }
                  this.charCtr.addChild(fighter.frame);
                  this._fighters.push(fighter);
               }
               else
               {
                  if(!alive || Boolean(infos && infos.hiddenInPrefight) && Boolean(isInPreFight))
                  {
                     numFighter++;
                  }
                  if(infos)
                  {
                     this._hiddenFighters[id] = {
                        "id":id,
                        "alive":alive,
                        "summoned":infos.summoned
                     };
                  }
               }
            }
            numFighter--;
            if(Boolean(fighter) && Boolean(fighter.hiddenInPrefight) && isInPreFight)
            {
               fighter.updateNumber(numFighter);
            }
            if(fighter)
            {
               fighter.updateUnsortedIcon();
            }
         }
         for each(fighter in oldFighters)
         {
            if(fighter)
            {
               fighter.frame.removeFromParent();
               if(deadFightersId.length > 0)
               {
                  fighter.destroy();
               }
               else
               {
                  fighter.destroy(true);
               }
            }
         }
         this.resizeTimeline();
         this.fightApi.updateSwapPositionRequestsIcons();
      }
      
      private function selectFighter(fighter:Fighter) : void
      {
         if(this._selectedFighter)
         {
            this._selectedFighter.selected = false;
         }
         if(this._selectedFighter != fighter)
         {
            this._selectedFighter = fighter;
            fighter.selected = true;
         }
         else
         {
            this._selectedFighter = null;
         }
      }
      
      private function setTurnOf(fighter:Fighter, waitTime:int, remainingTime:uint) : void
      {
         this._currentFighter = fighter;
         if(!fighter.risen)
         {
            fighter.risen = true;
         }
         fighter.startCountDown(waitTime,remainingTime);
         this.updateTurnOfArrow();
      }
      
      private function updateTurnOfArrow() : void
      {
         if(this._currentFighter)
         {
            if(!this.tx_currentArrow.visible)
            {
               this.tx_currentArrow.visible = true;
               this._prefightPhase = false;
            }
            this.tx_currentArrow.x = this._currentFighter.frame.x + int(this._currentFighter.frame.width / 2) - this.tx_currentArrow.height / 2;
         }
      }
      
      private function unsetTurnOf(fighter:Fighter) : void
      {
         if(this._currentFighter == fighter)
         {
            this._currentFighter = null;
         }
         fighter.risen = false;
         fighter.stopCountDown();
      }
      
      private function getFighterByFrame(frame:Object) : Fighter
      {
         var fighter:Fighter = null;
         for each(fighter in this._fighters)
         {
            if(fighter.frame == frame)
            {
               return fighter;
            }
         }
         return null;
      }
      
      public function getFighterById(id:Number) : Fighter
      {
         var fighter:Fighter = null;
         for each(fighter in this._fighters)
         {
            if(fighter.id == id)
            {
               return fighter;
            }
         }
         return null;
      }
      
      private function resizeTimeline() : void
      {
         var fighter:Fighter = null;
         var pos:int = 0;
         var frameOffsetHorizontal:int = this.uiApi.me().getConstant("frame_offset_horizontal");
         var currentX:int = 0;
         var timelineSize:int = 0;
         for(pos = 0; pos < this._fighters.length; pos++)
         {
            fighter = this._fighters[pos];
            if(fighter)
            {
               if(pos != 0)
               {
                  currentX += fighter.frame.width + frameOffsetHorizontal;
               }
               timelineSize += fighter.frame.width + frameOffsetHorizontal;
               fighter.frame.x = -currentX;
            }
         }
         var marginRight:int = this.uiApi.me().getConstant("bg_margin_right");
         var marginLeft:int = this.uiApi.me().getConstant("bg_margin_left");
         this.ctr_background.width = timelineSize + marginRight + marginLeft;
         if(this.ctr_background.width > this._screenWidth - 20)
         {
            this.ctr_background.width = this._screenWidth - 20;
         }
         this.refreshMoveOffset();
         if(this.isTimelineExtended())
         {
            this.btn_leftArrow.x = -this.ctr_background.width + int(this.uiApi.me().getConstant("bg_margin_left_to_arrow"));
            this.btn_leftArrow.visible = true;
            this.btn_rightArrow.visible = true;
            this._timelineMask.visible = true;
            this.charFrames.mask = this._timelineMask;
         }
         else
         {
            this.btn_leftArrow.visible = false;
            this.btn_rightArrow.visible = false;
            this._timelineMask.visible = false;
            this.charFrames.mask = null;
         }
         this.uiApi.me().render();
         this.updateTurnOfArrow();
      }
      
      private function isTimelineExtended() : Boolean
      {
         var fighter:Fighter = null;
         var extended:Boolean = false;
         var timelineSize:int = 0;
         var frameOffsetHorizontal:int = this.uiApi.me().getConstant("frame_offset_horizontal");
         var marginRight:int = this.uiApi.me().getConstant("bg_margin_right");
         var marginLeft:int = this.uiApi.me().getConstant("bg_margin_left");
         for each(fighter in this._fighters)
         {
            timelineSize += fighter.frame.width + frameOffsetHorizontal;
         }
         return timelineSize > this._screenWidth - marginRight - this.btn_minimArrow.width - marginLeft;
      }
      
      private function updateBuff(buffObj:*, targetId:Number) : void
      {
         var fighter:Fighter = this.getFighterById(targetId);
         if(fighter)
         {
            fighter.refreshPdv();
            fighter.refreshShield();
         }
      }
      
      private function updateFightersSprite(id:Number) : void
      {
         var fighter:Fighter = null;
         for each(fighter in this._fighters)
         {
            if(fighter.id == id)
            {
               fighter.updateSprite();
               fighter.refreshPdv();
            }
         }
      }
      
      private function updateAllFightersSprite() : void
      {
         var fighter:Fighter = null;
         for each(fighter in this._fighters)
         {
            fighter.updateSprite();
         }
      }
      
      private function onGameFightTurnListUpdated() : void
      {
         this.refreshTimeline();
      }
      
      private function onUpdatePreFightersList(id:Number = 0) : void
      {
         this.refreshTimeline();
         this.updateFightersSprite(id);
      }
      
      private function onUpdateFightersLook() : void
      {
         this.updateAllFightersSprite();
      }
      
      private function onGameFightTurnStart(id:Number, waitTime:int, remainingTime:uint, picture:Boolean) : void
      {
         var pos:* = 0;
         this._currentFighterWaitTime = id;
         this._currentFighterRemainingTime = id;
         var fighter:Fighter = this.getFighterById(id);
         if(fighter)
         {
            if(fighter.alive)
            {
               this.setTurnOf(fighter,waitTime,remainingTime);
            }
         }
         else
         {
            for(pos = int(this._fightersId.indexOf(id) - 1); pos > 0; )
            {
               fighter = this.getFighterById(this._fightersId[pos]);
               if(fighter)
               {
                  this.setTurnOf(fighter,waitTime,remainingTime);
                  break;
               }
               pos--;
            }
         }
      }
      
      private function onGameFightTurnEnd(id:Number) : void
      {
         var fighter:Fighter = this.getFighterById(id);
         if(Boolean(fighter) && fighter.alive)
         {
            this.unsetTurnOf(fighter);
         }
      }
      
      private function onGameFightPause(paused:Boolean) : void
      {
         if(paused)
         {
            this._currentFighter.pauseCountDown();
         }
      }
      
      private function onFightEvent(eventName:String, params:Object, targetList:Object = null) : void
      {
         var targetId:Number = NaN;
         var fighter:Fighter = null;
         var iSplice:int = 0;
         var iS:String = null;
         if(targetList == null)
         {
            targetList = new Array();
            if(params.length)
            {
               targetList[0] = params[0];
            }
         }
         var num:int = int(targetList.length);
         for(var i:int = 0; i < num; i++)
         {
            targetId = Number(targetList[i]);
            fighter = this.getFighterById(targetId);
            switch(eventName)
            {
               case "fighterLifeGain":
               case "fighterLifeLoss":
                  if(fighter)
                  {
                     fighter.refreshPdv();
                  }
                  break;
               case "fighterShieldLoss":
                  if(fighter)
                  {
                     fighter.refreshShield();
                  }
                  break;
               case "fighterGotDispelled":
               case "fighterTemporaryBoosted":
                  if(fighter)
                  {
                     fighter.refreshShield();
                     fighter.refreshPdv();
                  }
                  break;
               case "fighterDeath":
               case "fighterLeave":
                  if(fighter)
                  {
                     if(this._hideDeadGuys)
                     {
                        for(iS in this._fighters)
                        {
                           if(this._fighters[iS].id == fighter.id)
                           {
                              iSplice = int(iS);
                              break;
                           }
                        }
                        this._fighters.splice(iSplice,1);
                        this._hiddenFighters[fighter.id] = {
                           "id":fighter.id,
                           "alive":false,
                           "summoned":fighter.summoned
                        };
                        fighter.destroy(true);
                        this.charCtr.removeChild(fighter.frame);
                        this.resizeTimeline();
                        this.onOrderFightersSwitched(true);
                     }
                     else
                     {
                        fighter.alive = false;
                        this.onOrderFightersSwitched(true);
                        this.unsetTurnOf(fighter);
                     }
                  }
                  break;
               case "fighterChangeLook":
                  if(fighter)
                  {
                     fighter.look = params[1];
                  }
                  break;
               case "fighterSummoned":
            }
         }
      }
      
      private function onFoldAll(fold:Boolean) : void
      {
         this.folded = fold;
         this.btn_minimArrow.selected = fold;
      }
      
      public function onRelease(target:Object) : void
      {
         var fighter:Fighter = null;
         switch(target)
         {
            case this.btn_minimArrow:
               this.folded = !this.folded;
               break;
            case this.btn_leftArrow:
               this.moveLeft();
               break;
            case this.btn_rightArrow:
               this.moveRight();
               break;
            default:
               fighter = this.getFighterByFrame(target);
               if(!fighter)
               {
                  break;
               }
               if(this.fightApi.isCastingSpell())
               {
                  if(fighter.alive)
                  {
                     Api.sysApi.sendAction(new TimelineEntityClick(fighter.id));
                  }
               }
               else
               {
                  this.sysApi.dispatchHook(FighterSelected,fighter.id);
               }
         }
      }
      
      public function onFighterSelected(id:Number) : void
      {
         var fighter:Fighter = this.getFighterById(id);
         this.selectFighter(fighter);
      }
      
      public function onBuffAdd(buffId:uint, targetId:Number) : void
      {
         this.updateBuff(buffId,targetId);
      }
      
      public function onBuffRemove(buff:*, targetId:Number, reason:String) : void
      {
         this.updateBuff(buff,targetId);
      }
      
      public function onBuffUpdate(buffId:uint, targetId:Number) : void
      {
         this.updateBuff(buffId,targetId);
      }
      
      public function onBuffDispell(targetId:Number) : void
      {
         var fighter:Fighter = this.getFighterById(targetId);
         if(fighter)
         {
            fighter.refreshPdv();
            fighter.refreshShield();
         }
      }
      
      public function onOrderFightersSwitched(selected:Boolean) : void
      {
         var fighter:Fighter = null;
         var id:Number = NaN;
         var hiddenf:Object = null;
         var fightersId:Object = this.fightApi.getFighters();
         var isInPreFight:Boolean = Api.playerApi.isInPreFight();
         var num:uint = 0;
         var fighterExists:Boolean = false;
         for(var pos:int = 0; pos < fightersId.length; pos++)
         {
            id = Number(fightersId[pos]);
            num += 1;
            fighterExists = false;
            for each(fighter in this._fighters)
            {
               if(id == fighter.id)
               {
                  if(!fighter.alive || isInPreFight && fighter.hiddenInPrefight)
                  {
                     num--;
                  }
                  fighter.updateNumber(num);
                  fighterExists = true;
               }
            }
            if(!fighterExists)
            {
               hiddenf = this._hiddenFighters[id];
               if(Boolean(hiddenf) && Boolean(!hiddenf.alive) && !hiddenf.summoned)
               {
                  num--;
               }
            }
         }
      }
      
      public function onHideDeadFighters(selected:Boolean) : void
      {
         if(this._hideDeadGuys != selected)
         {
            this._hideDeadGuys = selected;
            this.refreshTimeline();
         }
      }
      
      public function onHideSummonedFighters(selected:Boolean) : void
      {
         if(this._hideSummonedStuff != selected)
         {
            this._hideSummonedStuff = selected;
            this.refreshTimeline();
            this.onOrderFightersSwitched(true);
         }
      }
      
      public function onTurnCountUpdated(turnCount:uint) : void
      {
         this._turnCount = turnCount;
         this.lbl_turnCount.text = this._turnCount.toString();
         if(this._turnCountBeforeNextWaveAttackers > -1)
         {
            --this._turnCountBeforeNextWaveAttackers;
         }
         if(this._turnCountBeforeNextWaveDefenders > -1)
         {
            --this._turnCountBeforeNextWaveDefenders;
         }
      }
      
      public function onWaveUpdated(teamId:int, wave:int, nbTurnBeforeNext:int) : void
      {
         var total:int = int(wave / 10);
         if(total > 0)
         {
            wave %= 10;
         }
         else if(wave == 1)
         {
            nbTurnBeforeNext--;
         }
         if(teamId == TeamEnum.TEAM_CHALLENGER)
         {
            this._waveCountAttackers = wave;
            this._waveTotalAttackers = total;
            this._turnCountBeforeNextWaveAttackers = nbTurnBeforeNext;
         }
         else if(teamId == TeamEnum.TEAM_DEFENDER)
         {
            this._waveCountDefenders = wave;
            this._waveTotalDefenders = total;
            this._turnCountBeforeNextWaveDefenders = nbTurnBeforeNext;
         }
         this.tx_wave.visible = true;
      }
      
      private function texteVague(wave:int, total:int, tours:int) : String
      {
         var restantes:int = 0;
         var t:String = this.uiApi.getText("ui.common.wave") + " " + wave;
         if(total > 0)
         {
            restantes = total - wave;
            t += "/" + total;
            if(restantes > 1)
            {
               t += " (" + restantes + " vagues restantes)";
            }
            else if(restantes == 1)
            {
               t += " (1 vague restante)";
            }
            else
            {
               t += " (dernière vague)";
            }
         }
         return t;
      }

      public function onRollOver(target:Object) : void
      {
         var text:String = null;
         var text2:String = null;
         var fighter:Fighter = null;
         var frameGlobalPos:Point = null;
         var frameRectangle:TooltipRectangle = null;
         this._rollOverMe = true;
         if(target == this.lbl_turnCount)
         {
            text = this.uiApi.getText("ui.fight.turnNumber",this._turnCount);
            this.uiApi.showTooltip(this.uiApi.textTooltipInfo(text),target,false,"standard",7,1,0,null,null,null,"TextInfo");
         }
         else if(target == this.tx_wave && (this._waveCountAttackers > 0 || this._waveCountDefenders > 0))
         {
            text2 = "";
            if(this._waveCountAttackers > 0)
            {
               text2 += this.uiApi.getText("ui.common.attackers") + "\n   " + this.texteVague(this._waveCountAttackers,this._waveTotalAttackers,this._turnCountBeforeNextWaveAttackers);
               if(this._turnCountBeforeNextWaveAttackers == 0)
               {
                  text2 += "\n   " + this.uiApi.getText("ui.fight.newWaveIncoming");
               }
               else if(this._turnCountBeforeNextWaveAttackers > 0)
               {
                  text2 += "\n   " + this.uiApi.processText(this.uiApi.getText("ui.fight.turnsBeforeNextWave",this._turnCountBeforeNextWaveAttackers),"",this._turnCountBeforeNextWaveAttackers == 1);
               }
            }
            if(this._waveCountDefenders > 0)
            {
               text2 += this.uiApi.getText("ui.common.defenders") + "\n   " + this.texteVague(this._waveCountDefenders,this._waveTotalDefenders,this._turnCountBeforeNextWaveDefenders);
               if(this._turnCountBeforeNextWaveDefenders == 0)
               {
                  text2 += "\n   " + this.uiApi.getText("ui.fight.newWaveIncoming");
               }
               else if(this._turnCountBeforeNextWaveDefenders > 0)
               {
                  text2 += "\n   " + this.uiApi.processText(this.uiApi.getText("ui.fight.turnsBeforeNextWave",this._turnCountBeforeNextWaveDefenders),"",this._turnCountBeforeNextWaveDefenders == 1);
               }
            }
            this.uiApi.showTooltip(this.uiApi.textTooltipInfo(text2),target,false,"standard",7,1,0,null,null,null,"TextInfo");
         }
         else
         {
            fighter = this.getFighterByFrame(target);
            if(!fighter)
            {
               return;
            }
            if(fighter.hiddenInPrefight && this.playerApi.isInPreFight())
            {
               frameGlobalPos = target.parent.localToGlobal(new Point(target.x,target.y));
               frameRectangle = this.tooltipApi.createTooltipRectangle(frameGlobalPos.x,frameGlobalPos.y,target.width,target.height);
               this.sysApi.sendAction(new TimelineEntityOver(fighter.id,true,true,frameRectangle));
            }
            else
            {
               this.sysApi.sendAction(new TimelineEntityOver(fighter.id,true));
            }
         }
      }
      
      public function onRollOut(target:Object) : void
      {
         var fighter:Fighter = null;
         this._rollOverMe = false;
         if(target == this.lbl_turnCount || target == this.tx_wave)
         {
            this.uiApi.hideTooltip();
         }
         else
         {
            fighter = this.getFighterByFrame(target);
            if(fighter)
            {
               this.sysApi.sendAction(new TimelineEntityOut(fighter.id));
            }
         }
      }
      
      private function onFighterMouseAction(infos:Object = null) : void
      {
         var fighter:Fighter = null;
         if(infos)
         {
            fighter = this.getFighterById(infos.contextualId);
            if(fighter != null && fighter.alive)
            {
               fighter.highlight = true;
               if(!this._rollOverMe)
               {
                  fighter.risen = true;
               }
            }
         }
         else
         {
            for each(fighter in this._fighters)
            {
               if(!fighter.isCurrentPlayer)
               {
                  fighter.risen = false;
               }
               fighter.highlight = false;
            }
         }
         this.fightApi.updateSwapPositionRequestsIcons();
      }
   }
}

