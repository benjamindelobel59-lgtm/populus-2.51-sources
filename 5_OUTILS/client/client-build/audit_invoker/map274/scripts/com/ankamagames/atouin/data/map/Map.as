package com.ankamagames.atouin.data.map
{
   import com.ankamagames.atouin.data.DataFormatError;
   import com.ankamagames.atouin.data.elements.Elements;
   import com.ankamagames.atouin.data.elements.GraphicalElementData;
   import com.ankamagames.atouin.data.elements.subtypes.NormalGraphicalElementData;
   import com.ankamagames.atouin.data.map.elements.BasicElement;
   import com.ankamagames.atouin.data.map.elements.GraphicalElement;
   import com.ankamagames.jerakine.logger.Log;
   import com.ankamagames.jerakine.logger.Logger;
   import flash.errors.IllegalOperationError;
   import flash.utils.ByteArray;
   import flash.utils.IDataInput;
   import flash.utils.getQualifiedClassName;
   
   public class Map
   {
      
      private static var decryptionKey:ByteArray = null;
      
      protected static const _log:Logger = Log.getLogger(getQualifiedClassName(Map));
      
      public var mapVersion:int;
      
      public var encrypted:Boolean;
      
      public var encryptionVersion:uint;
      
      public var groundCRC:int;
      
      public var zoomScale:Number = 1;
      
      public var zoomOffsetX:int;
      
      public var zoomOffsetY:int;
      
      public var groundCacheCurrentlyUsed:int = 0;
      
      public var id:Number;
      
      public var relativeId:int;
      
      public var mapType:int;
      
      public var backgroundsCount:int;
      
      public var backgroundFixtures:Vector.<Fixture>;
      
      public var foregroundsCount:int;
      
      public var foregroundFixtures:Vector.<Fixture>;
      
      public var subareaId:int;
      
      public var shadowBonusOnEntities:int;
      
      public var gridColor:uint;
      
      public var backgroundColor:uint;
      
      public var backgroundRed:int;
      
      public var backgroundGreen:int;
      
      public var backgroundBlue:int;
      
      public var backgroundAlpha:int = 0;
      
      public var topNeighbourId:Number;
      
      public var bottomNeighbourId:Number;
      
      public var leftNeighbourId:Number;
      
      public var rightNeighbourId:Number;
      
      public var cellsCount:int;
      
      public var layersCount:int;
      
      public var isUsingNewMovementSystem:Boolean = false;
      
      public var layers:Vector.<Layer>;
      
      public var cells:Vector.<CellData>;
      
      public var topArrowCell:Vector.<uint> = new Vector.<uint>();
      
      public var leftArrowCell:Vector.<uint> = new Vector.<uint>();
      
      public var bottomArrowCell:Vector.<uint> = new Vector.<uint>();
      
      public var rightArrowCell:Vector.<uint> = new Vector.<uint>();
      
      private var _parsed:Boolean;
      
      private var _failed:Boolean;
      
      private var _gfxList:Vector.<NormalGraphicalElementData>;
      
      private var _gfxCount:Array;
      
      public var tacticalModeTemplateId:int;
      
      public function Map()
      {
         super();
      }
      
      public function get parsed() : Boolean
      {
         return this._parsed;
      }
      
      public function get failed() : Boolean
      {
         return this._failed;
      }
      
      public function getGfxList(param1:Boolean = false) : Vector.<NormalGraphicalElementData>
      {
         if(!this._gfxList)
         {
            this.computeGfxList(param1);
         }
         return this._gfxList;
      }
      
      public function getGfxCount(param1:uint) : uint
      {
         if(!this._gfxList)
         {
            this.computeGfxList();
         }
         return this._gfxCount[param1];
      }
      
      public function fromRaw(param1:IDataInput) : void
      {
         var _loc6_:int = 0;
         var _loc15_:int = 0;
         var _loc5_:Fixture = null;
         var _loc12_:Layer = null;
         var _loc11_:* = 0;
         var _loc2_:CellData = null;
         var _loc9_:* = 0;
         var _loc13_:ByteArray = null;
         var _loc14_:int = 0;
         var _loc10_:int = 0;
         var _loc7_:int = 0;
         var _loc4_:int = 0;
         var _loc8_:int = 0;
         var _loc3_:Fixture = null;
         if(decryptionKey == null)
         {
            decryptionKey = new ByteArray();
            decryptionKey.writeMultiByte("649ae451ca33ec53bbcbcc33becf15f4","ascii");
         }
         try
         {
            _loc15_ = param1.readByte();
            if(_loc15_ != 77)
            {
               throw new DataFormatError("Unknown file format");
            }
            this.mapVersion = param1.readByte();
            this.id = param1.readUnsignedInt();
            if(this.mapVersion >= 7)
            {
               this.encrypted = param1.readBoolean();
               this.encryptionVersion = param1.readByte();
               _loc9_ = uint(param1.readInt());
               if(this.encrypted)
               {
                  if(!decryptionKey)
                  {
                     throw new IllegalOperationError("Map decryption key is empty");
                  }
                  _loc13_ = new ByteArray();
                  param1.readBytes(_loc13_,0,_loc9_);
                  _loc6_ = 0;
                  while(_loc6_ < _loc13_.length)
                  {
                     _loc13_[_loc6_] ^= decryptionKey[_loc6_ % decryptionKey.length];
                     _loc6_ += 1;
                  }
                  _loc13_.position = 0;
                  param1 = _loc13_;
               }
            }
            this.relativeId = param1.readUnsignedInt();
            this.mapType = param1.readByte();
            this.subareaId = param1.readInt();
            this.topNeighbourId = param1.readInt();
            this.bottomNeighbourId = param1.readInt();
            this.leftNeighbourId = param1.readInt();
            this.rightNeighbourId = param1.readInt();
            this.shadowBonusOnEntities = param1.readUnsignedInt();
            if(this.mapVersion >= 9)
            {
               _loc14_ = param1.readInt();
               this.backgroundAlpha = (_loc14_ & 0xFF000000) >> 32;
               this.backgroundRed = (_loc14_ & 0xFF0000) >> 16;
               this.backgroundGreen = (_loc14_ & 0xFF00) >> 8;
               this.backgroundBlue = _loc14_ & 0xFF;
               _loc14_ = int(param1.readUnsignedInt());
               _loc10_ = (_loc14_ & 0xFF000000) >> 32;
               _loc7_ = (_loc14_ & 0xFF0000) >> 16;
               _loc4_ = (_loc14_ & 0xFF00) >> 8;
               _loc8_ = _loc14_ & 0xFF;
               this.gridColor = (_loc10_ & 0xFF) << 32 | (_loc7_ & 0xFF) << 16 | (_loc4_ & 0xFF) << 8 | _loc8_ & 0xFF;
            }
            else if(this.mapVersion >= 3)
            {
               this.backgroundRed = param1.readByte();
               this.backgroundGreen = param1.readByte();
               this.backgroundBlue = param1.readByte();
            }
            this.backgroundColor = (this.backgroundAlpha & 0xFF) << 32 | (this.backgroundRed & 0xFF) << 16 | (this.backgroundGreen & 0xFF) << 8 | this.backgroundBlue & 0xFF;
            if(this.mapVersion >= 4)
            {
               this.zoomScale = param1.readUnsignedShort() / 100;
               this.zoomOffsetX = param1.readShort();
               this.zoomOffsetY = param1.readShort();
               if(this.zoomScale < 1)
               {
                  this.zoomScale = 1;
                  this.zoomOffsetX = this.zoomOffsetY = 0;
               }
            }
            if(this.mapVersion > 10)
            {
               this.tacticalModeTemplateId = param1.readInt();
            }
            this.backgroundsCount = param1.readByte();
            this.backgroundFixtures = new Vector.<Fixture>(this.backgroundsCount,true);
            _loc6_ = 0;
            while(_loc6_ < this.backgroundsCount)
            {
               _loc5_ = new Fixture(this);
               _loc5_.fromRaw(param1);
               this.backgroundFixtures[_loc6_] = _loc5_;
               _loc6_ += 1;
            }
            this.foregroundsCount = param1.readByte();
            this.foregroundFixtures = new Vector.<Fixture>(this.foregroundsCount,true);
            _loc6_ = 0;
            while(_loc6_ < this.foregroundsCount)
            {
               _loc3_ = new Fixture(this);
               _loc3_.fromRaw(param1);
               this.foregroundFixtures[_loc6_] = _loc3_;
               _loc6_ += 1;
            }
            this.cellsCount = 560;
            param1.readInt();
            this.groundCRC = param1.readInt();
            this.layersCount = param1.readByte();
            this.layers = new Vector.<Layer>(this.layersCount,true);
            _loc6_ = 0;
            while(_loc6_ < this.layersCount)
            {
               _loc12_ = new Layer(this);
               _loc12_.fromRaw(param1,this.mapVersion);
               this.layers[_loc6_] = _loc12_;
               _loc6_ += 1;
            }
            this.cells = new Vector.<CellData>(this.cellsCount,true);
            _loc6_ = 0;
            while(_loc6_ < this.cellsCount)
            {
               _loc2_ = new CellData(this,_loc6_);
               _loc2_.fromRaw(param1);
               if(!_loc11_)
               {
                  _loc11_ = _loc2_.moveZone;
               }
               if(_loc2_.moveZone != _loc11_)
               {
                  this.isUsingNewMovementSystem = true;
               }
               this.cells[_loc6_] = _loc2_;
               _loc6_ += 1;
            }
            this.topArrowCell.fixed = true;
            this.leftArrowCell.fixed = true;
            this.bottomArrowCell.fixed = true;
            this.rightArrowCell.fixed = true;
            this._parsed = true;
         }
         catch(e:*)
         {
            _failed = true;
            throw e;
         }
      }
      
      private function computeGfxList(param1:Boolean = false) : void
      {
         var _loc7_:int = 0;
         var _loc3_:int = 0;
         var _loc4_:int = 0;
         var _loc12_:* = undefined;
         var _loc15_:int = 0;
         var _loc11_:* = undefined;
         var _loc6_:int = 0;
         var _loc10_:Layer = null;
         var _loc8_:Cell = null;
         var _loc17_:BasicElement = null;
         var _loc2_:int = 0;
         var _loc5_:GraphicalElementData = null;
         var _loc9_:NormalGraphicalElementData = null;
         var _loc13_:* = null;
         var _loc18_:Elements = Elements.getInstance();
         var _loc14_:Array = [];
         this._gfxCount = [];
         var _loc16_:int = int(this.layers.length);
         _loc7_ = 0;
         while(_loc7_ < _loc16_)
         {
            _loc10_ = this.layers[_loc7_];
            if(!(param1 && _loc7_ == 0))
            {
               _loc12_ = _loc10_.cells;
               _loc15_ = int(_loc12_.length);
               _loc3_ = 0;
               while(_loc3_ < _loc15_)
               {
                  _loc8_ = _loc12_[_loc3_];
                  _loc11_ = _loc8_.elements;
                  _loc6_ = int(_loc11_.length);
                  _loc4_ = 0;
                  while(_loc4_ < _loc6_)
                  {
                     _loc17_ = _loc11_[_loc4_];
                     if(_loc17_.elementType == 2)
                     {
                        _loc2_ = int(GraphicalElement(_loc17_).elementId);
                        _loc5_ = _loc18_.getElementData(_loc2_);
                        if(_loc5_ == null)
                        {
                           _log.error("Unknown graphical element ID " + _loc2_);
                        }
                        else if(_loc5_ is NormalGraphicalElementData)
                        {
                           _loc9_ = _loc5_ as NormalGraphicalElementData;
                           _loc14_[_loc9_.gfxId] = _loc9_;
                           if(this._gfxCount[_loc9_.gfxId])
                           {
                              this._gfxCount[_loc9_.gfxId]++;
                           }
                           else
                           {
                              this._gfxCount[_loc9_.gfxId] = 1;
                           }
                        }
                     }
                     _loc4_++;
                  }
                  _loc3_++;
               }
            }
            _loc7_++;
         }
         this._gfxList = new Vector.<NormalGraphicalElementData>();
         for each(_loc13_ in _loc14_)
         {
            this._gfxList.push(_loc13_);
         }
         this._gfxList.fixed = true;
      }
   }
}

