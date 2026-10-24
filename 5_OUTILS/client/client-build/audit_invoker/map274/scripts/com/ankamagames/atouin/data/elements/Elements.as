package com.ankamagames.atouin.data.elements
{
   import com.ankamagames.atouin.data.DataFormatError;
   import com.ankamagames.jerakine.logger.Log;
   import com.ankamagames.jerakine.logger.Logger;
   import com.ankamagames.jerakine.utils.errors.SingletonError;
   import flash.utils.Dictionary;
   import flash.utils.IDataInput;
   import flash.utils.getQualifiedClassName;
   
   public class Elements
   {
      
      private static var _self:Elements;
      
      protected static const _log:Logger = Log.getLogger(getQualifiedClassName(Elements));
      
      public var fileVersion:uint;
      
      public var elementsCount:uint;
      
      private var _parsed:Boolean;
      
      private var _failed:Boolean;
      
      private var _elementsMap:Dictionary;
      
      private var _jpgMap:Dictionary;
      
      private var _elementsIndex:Dictionary;
      
      private var _rawData:IDataInput;
      
      public function Elements()
      {
         super();
         if(_self)
         {
            throw new SingletonError();
         }
      }
      
      public static function getInstance() : Elements
      {
         if(!_self)
         {
            _self = new Elements();
         }
         return _self;
      }
      
      public function get parsed() : Boolean
      {
         return this._parsed;
      }
      
      public function get failed() : Boolean
      {
         return this._failed;
      }
      
      public function getElementData(param1:int) : GraphicalElementData
      {
         return this._elementsMap[param1] ? GraphicalElementData(this._elementsMap[param1]) : this.readElement(param1);
      }
      
      public function isJpg(param1:uint) : Boolean
      {
         return this._jpgMap[param1] == true;
      }
      
      public function fromRaw(param1:IDataInput) : void
      {
         var _loc5_:int = 0;
         var _loc3_:* = 0;
         var _loc6_:int = 0;
         var _loc2_:int = 0;
         var _loc4_:int = 0;
         var _loc7_:int = 0;
         try
         {
            _loc5_ = param1.readByte();
            if(_loc5_ != 69)
            {
               throw new DataFormatError("Unknown file format");
            }
            this._rawData = param1;
            this.fileVersion = param1.readByte();
            this.elementsCount = param1.readUnsignedInt();
            this._elementsMap = new Dictionary();
            this._elementsIndex = new Dictionary();
            _loc3_ = 0;
            _loc6_ = 0;
            while(_loc6_ < this.elementsCount)
            {
               if(this.fileVersion >= 9)
               {
                  _loc3_ = param1.readUnsignedShort();
               }
               _loc2_ = param1.readInt();
               if(this.fileVersion <= 8)
               {
                  this._elementsIndex[_loc2_] = param1["position"];
                  this.readElement(_loc2_);
               }
               else
               {
                  this._elementsIndex[_loc2_] = param1["position"];
                  param1["position"] += _loc3_ - 4;
               }
               _loc6_ += 1;
            }
            if(this.fileVersion >= 8)
            {
               _loc4_ = param1.readInt();
               this._jpgMap = new Dictionary();
               _loc6_ = 0;
               while(_loc6_ < _loc4_)
               {
                  _loc7_ = param1.readInt();
                  this._jpgMap[_loc7_] = true;
                  _loc6_ += 1;
               }
            }
            this._parsed = true;
         }
         catch(e:*)
         {
            _failed = true;
            throw e;
         }
      }
      
      private function readElement(param1:uint) : GraphicalElementData
      {
         this._rawData["position"] = this._elementsIndex[param1];
         var _loc2_:int = this._rawData.readByte();
         var _loc3_:GraphicalElementData = GraphicalElementFactory.getGraphicalElementData(param1,_loc2_);
         if(!_loc3_)
         {
            return null;
         }
         _loc3_.fromRaw(this._rawData,this.fileVersion);
         this._elementsMap[param1] = _loc3_;
         return _loc3_;
      }
   }
}

