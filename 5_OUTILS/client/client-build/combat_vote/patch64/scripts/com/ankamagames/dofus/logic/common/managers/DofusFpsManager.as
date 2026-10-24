package com.ankamagames.dofus.logic.common.managers
{
   import com.ankamagames.dofus.BuildInfos;
   import com.ankamagames.dofus.misc.interClient.InterClientManager;
   import com.ankamagames.dofus.network.enums.BuildTypeEnum;
   import com.ankamagames.atouin.entities.behaviours.movements.AnimatedMovementBehavior;
   import com.ankamagames.dofus.logic.game.common.managers.PlayedCharacterManager;
   import com.ankamagames.dofus.types.DofusOptions;
   import com.ankamagames.jerakine.managers.OptionManager;
   import com.ankamagames.dofus.types.data.FpsLogWrapper;
   import com.ankamagames.jerakine.managers.PerformanceManager;
   import com.ankamagames.jerakine.utils.display.EnterFrameDispatcher;
   import com.ankamagames.jerakine.utils.display.FpsControler;
   import com.ankamagames.jerakine.utils.display.StageShareManager;
   import com.ankamagames.tiphon.engine.TiphonCacheManager;
   import com.ankamagames.tiphon.engine.TiphonDebugManager;
   import flash.desktop.NativeApplication;
   import flash.display.NativeWindow;
   import flash.display.NativeWindowDisplayState;
   import flash.events.Event;
   import flash.events.MouseEvent;
   import flash.events.NativeWindowDisplayStateEvent;
   import flash.utils.getTimer;
   
   public class DofusFpsManager
   {
      
      public static var currentFps:Number;
      
      private static var _elapsedTime:uint;
      
      private static var _lastTime:uint;
      
      private static var _frame:uint;
      
      private static var _logWrapped:FpsLogWrapper;
      
      private static var _logRamWrapped:FpsLogWrapper;
      
      private static var _totalFrame:int = 0;
      
      private static var _animFPS:int = 25;
      
      private static var _interval:int = 1000 / _animFPS;
      
      private static var _framePlayed:int = 0;
      
      private static var _frameNeeded:int = 0;
      
      private static var _focusListInfo:Array = new Array();
      
      public static var allowSkipFrame:Boolean = true;
      
      // PADGREF 23/10 : vitesse des combats (option « vitesseCombat » : 2 par défaut, 1 = normale). En combat, les animations
      // (personnages, sorts) avancent selon un temps accéléré et les déplacements vont plus vite (AnimatedMovementBehavior).
      private static var _tempsAnim:Number = 0;
      
      public static function vitesseCombat() : Number
      {
         var v:Number = 1;
         try
         {
            if(PlayedCharacterManager.getInstance().isFighting)
            {
               v = Number(OptionManager.getOptionManager("dofus")["vitesseCombat"]);
            }
         }
         catch(e:Error)
         {
            v = 1;
         }
         if(isNaN(v) || v < 1)
         {
            v = 1;
         }
         return v > 4 ? 4 : v;
      }
      
      public function DofusFpsManager()
      {
         super();
      }
      
      public static function init() : void
      {
         EnterFrameDispatcher.addEventListener(onEnterFrame,"DofusFpsManager");
         StageShareManager.stage.addEventListener(Event.ACTIVATE,onActivate);
         StageShareManager.stage.addEventListener(Event.DEACTIVATE,onDesactivate);
         NativeApplication.nativeApplication.openedWindows[0].addEventListener(NativeWindowDisplayStateEvent.DISPLAY_STATE_CHANGE,onStateChange);
         TiphonCacheManager.init();
         _logWrapped = new FpsLogWrapper();
         _logRamWrapped = new FpsLogWrapper();
         if(BuildInfos.BUILD_TYPE == BuildTypeEnum.DEBUG || BuildInfos.BUILD_TYPE == BuildTypeEnum.INTERNAL)
         {
            TiphonDebugManager.enable();
         }
      }
      
      public static function updateFocusList(focusList:Array, clientId:String) : void
      {
         var lastFocus:String = null;
         var lastTime:Number = NaN;
         var time:Number = NaN;
         var nativeWindow:NativeWindow = NativeApplication.nativeApplication.openedWindows[0];
         if(Boolean(nativeWindow) && nativeWindow["displayState"] == NativeWindowDisplayState.MINIMIZED)
         {
            StageShareManager.stage.frameRate = 12;
            return;
         }
         var num:int = int(focusList.length);
         for(var i:int = 0; i < num; )
         {
            if(lastFocus == null)
            {
               lastFocus = focusList[i];
               lastTime = Number(focusList[i + 1]);
            }
            else
            {
               time = Number(focusList[i + 1]);
               if(lastTime < time)
               {
                  lastFocus = focusList[i];
                  lastTime = time;
               }
            }
            i += 2;
         }
         if(clientId == lastFocus)
         {
            StageShareManager.stage.frameRate = PerformanceManager.BASE_FRAMERATE;
         }
         else if(!StageShareManager.isActive)
         {
            StageShareManager.stage.frameRate = 12;
         }
      }
      
      private static function onActivate(e:Event) : void
      {
         StageShareManager.stage.frameRate = PerformanceManager.BASE_FRAMERATE;
         var options:DofusOptions = Dofus.getInstance().options;
         if(Boolean(options) && Boolean(options.optimizeMultiAccount))
         {
            InterClientManager.getInstance().gainFocus();
         }
         StageShareManager.stage.removeEventListener(MouseEvent.MOUSE_OVER,onStageRollOver);
      }
      
      private static function onDesactivate(e:Event) : void
      {
         StageShareManager.stage.addEventListener(MouseEvent.MOUSE_OVER,onStageRollOver);
      }
      
      private static function onStageRollOver(e:Event) : void
      {
         StageShareManager.stage.removeEventListener(MouseEvent.MOUSE_OVER,onStageRollOver);
         StageShareManager.stage.frameRate = PerformanceManager.BASE_FRAMERATE;
      }
      
      private static function onStateChange(e:NativeWindowDisplayStateEvent) : void
      {
         var options:DofusOptions = Dofus.getInstance().options;
         if(Boolean(options) && Boolean(options.optimizeMultiAccount))
         {
            if(e.afterDisplayState == NativeWindowDisplayState.MINIMIZED)
            {
               StageShareManager.stage.frameRate = 12;
               InterClientManager.getInstance().resetFocus();
            }
            else
            {
               StageShareManager.stage.frameRate = PerformanceManager.BASE_FRAMERATE;
            }
         }
      }
      
      private static function onEnterFrame(e:Event) : void
      {
         var numFrame:int = 0;
         var i:int = 0;
         var time:int = getTimer();
         _elapsedTime += time - _lastTime;
         ++_frame;
         if(_elapsedTime > 1000)
         {
            currentFps = _frame / (_elapsedTime / 1000);
            _elapsedTime = 0;
            _frame = 0;
         }
         var vitesse:Number = vitesseCombat();
         AnimatedMovementBehavior.multiplicateurVitesse = vitesse;
         var ecart:int = time - _lastTime;
         if(ecart < 0 || ecart > 250)
         {
            ecart = _interval;
         }
         _tempsAnim += ecart * vitesse;
         if(allowSkipFrame)
         {
            _frameNeeded = _tempsAnim / _interval;
            ++_totalFrame;
            numFrame = _frameNeeded - _framePlayed;
            if(numFrame > 12)
            {
               numFrame = 12;
            }
            if(numFrame)
            {
               _framePlayed = _frameNeeded;
               for(i = 0; i < numFrame; i++)
               {
                  FpsControler.nextFrame();
               }
            }
         }
         else
         {
            _frameNeeded = 1;
            ++_totalFrame;
            FpsControler.nextFrame();
            if(vitesse >= 2)
            {
               FpsControler.nextFrame();
            }
         }
         _lastTime = time;
      }
   }
}

