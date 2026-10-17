package com.ankamagames.jerakine.utils.display
{
   import com.ankamagames.jerakine.logger.Log;
   import com.ankamagames.jerakine.logger.Logger;
   import flash.events.Event;
   import flash.utils.Dictionary;
   import flash.utils.getQualifiedClassName;
   import flash.utils.getTimer;
   
   public class EnterFrameDispatcher
   {
      
      private static var _listenerUp:Boolean;
      
      private static var _currentTime:uint;
      
      private static var _listenersCount:uint;
      
      protected static const _log:Logger = Log.getLogger(getQualifiedClassName(EnterFrameDispatcher));
      
      private static var _controledListeners:Dictionary = new Dictionary(true);
      
      private static var _realTimeListeners:Dictionary = new Dictionary();

      public static var diagLastListener:String = "";

      private static var _diagWrappers:Dictionary = new Dictionary();

      private static function diagWrap(listener:Function, name:String) : Function
      {
         return function(e:Event):void
         {
            var t:int = getTimer();
            diagLastListener = name;
            listener(e);
            t = getTimer() - t;
            if(t > 150)
            {
               _log.warn("[DIAG LENT] tache image \'" + name + "\' en " + t + " ms");
            }
         };
      }

      public function EnterFrameDispatcher()
      {
         super();
      }
      
      public static function get enterFrameListenerCount() : uint
      {
         return _listenersCount;
      }
      
      public static function get controledEnterFrameListeners() : Dictionary
      {
         return _controledListeners;
      }
      
      public static function get realTimeEnterFrameListeners() : Dictionary
      {
         return _realTimeListeners;
      }
      
      public static function addEventListener(listener:Function, name:String, frameRate:uint = 4294967295) : void
      {
         if(frameRate == uint.MAX_VALUE || frameRate >= StageShareManager.stage.frameRate)
         {
            _realTimeListeners[listener] = name;
            if(!_diagWrappers[listener])
            {
               _diagWrappers[listener] = diagWrap(listener,name);
               StageShareManager.rootContainer.addEventListener(Event.ENTER_FRAME,_diagWrappers[listener]);
            }
         }
         else if(!_controledListeners[listener])
         {
            _controledListeners[listener] = new ControledEnterFrameListener(name,listener,frameRate > 0 ? uint(1000 / frameRate) : 0,_listenerUp ? _currentTime : uint(getTimer()));
            if(!_listenerUp)
            {
               StageShareManager.rootContainer.addEventListener(Event.ENTER_FRAME,onEnterFrame);
               _listenerUp = true;
            }
         }
         ++_listenersCount;
      }
      
      public static function hasEventListener(listener:Function) : Boolean
      {
         return _controledListeners[listener] != null || _realTimeListeners[listener] != null;
      }
      
      public static function removeEventListener(listener:Function) : void
      {
         var k:* = undefined;
         if(_controledListeners[listener])
         {
            delete _controledListeners[listener];
            --_listenersCount;
         }
         if(_realTimeListeners[listener])
         {
            delete _realTimeListeners[listener];
            if(_diagWrappers[listener])
            {
               StageShareManager.rootContainer.removeEventListener(Event.ENTER_FRAME,_diagWrappers[listener],false);
               delete _diagWrappers[listener];
            }
            --_listenersCount;
         }
         var _loc3_:int = 0;
         var _loc4_:* = _controledListeners;
         for each(k in _loc4_)
         {
         }
         if(StageShareManager.rootContainer)
         {
            StageShareManager.rootContainer.removeEventListener(Event.ENTER_FRAME,onEnterFrame);
         }
         _listenerUp = false;
      }
      
      private static function onEnterFrame(e:Event) : void
      {
         var cefl:ControledEnterFrameListener = null;
         var diff:uint = 0;
         _currentTime = getTimer();
         for each(cefl in _controledListeners)
         {
            diff = _currentTime - cefl.latestChange;
            if(diff > cefl.wantedGap - cefl.overhead)
            {
               diagLastListener = cefl.name;
               var diagT:int = getTimer();
               cefl.listener(e);
               diagT = getTimer() - diagT;
               if(diagT > 150)
               {
                  _log.warn("[DIAG LENT] tache image \'" + cefl.name + "\' en " + diagT + " ms");
               }
               cefl.latestChange = _currentTime;
               cefl.overhead = diff - cefl.wantedGap + cefl.overhead;
            }
         }
      }
   }
}

class ControledEnterFrameListener
{
   
   public var name:String;
   
   public var listener:Function;
   
   public var wantedGap:uint;
   
   public var overhead:uint;
   
   public var latestChange:uint;
   
   public function ControledEnterFrameListener(name:String, listener:Function, wantedGap:uint, latestChange:uint)
   {
      super();
      this.name = name;
      this.listener = listener;
      this.wantedGap = wantedGap;
      this.latestChange = latestChange;
   }
}
