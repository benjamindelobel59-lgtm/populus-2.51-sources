package com.ankamagames.berilia.components
{
   import com.ankamagames.berilia.types.graphic.GraphicContainer;
   import flash.display.DisplayObject;
   
   public class Grid extends GraphicContainer
   {
      
      public function Grid()
      {
         super();
      }
      
      public function set hPadding(value:int) : void
      {
      }
      
      public function get hPadding() : int
      {
         return 0;
      }
      
      public function set vPadding(value:int) : void
      {
      }
      
      public function get vPadding() : int
      {
         return 0;
      }
      
      public function set allowLastToFirst(value:Boolean) : void
      {
      }
      
      public function get allowLastToFirst() : Boolean
      {
         return false;
      }
      
      public function set useLeftRightToSelect(value:Boolean) : void
      {
      }
      
      public function get useLeftRightToSelect() : Boolean
      {
         return false;
      }
      
      public function get scrollBarSize() : uint
      {
         return 0;
      }
      
      public function set rendererName(value:String) : void
      {
      }
      
      public function get rendererName() : String
      {
         return null;
      }
      
      public function set rendererArgs(value:String) : void
      {
      }
      
      public function get rendererArgs() : String
      {
         return null;
      }
      
      public function get renderer() : Object
      {
         return null;
      }
      
      public function set dataProvider(data:*) : void
      {
      }
      
      public function get dataProvider() : *
      {
         return null;
      }
      
      public function resetScrollBar() : void
      {
      }
      
      public function set horizontalScrollbarCss(sValue:Object) : void
      {
      }
      
      public function get horizontalScrollbarCss() : Object
      {
         return null;
      }
      
      public function set verticalScrollbarCss(sValue:Object) : void
      {
      }
      
      public function get verticalScrollbarCss() : Object
      {
         return null;
      }
      
      public function set scrollbarOffset(sValue:int) : void
      {
      }
      
      public function get scrollbarOffset() : int
      {
         return 0;
      }
      
      public function get selectedIndex() : int
      {
         return 0;
      }
      
      public function set selectedIndex(i:int) : void
      {
      }
      
      public function set verticalScroll(b:Boolean) : void
      {
      }
      
      public function get verticalScroll() : Boolean
      {
         return false;
      }
      
      public function get pageYOffset() : int
      {
         return 0;
      }
      
      public function set autoSelect(b:Boolean) : void
      {
      }
      
      public function get autoSelect() : Boolean
      {
         return false;
      }
      
      public function set autoSelectMode(mode:int) : void
      {
      }
      
      public function get autoSelectMode() : int
      {
         return 0;
      }
      
      public function get scrollDisplay() : String
      {
         return null;
      }
      
      public function set scrollDisplay(sValue:String) : void
      {
      }
      
      public function get pagesCount() : uint
      {
         return 0;
      }
      
      public function get selectedItem() : *
      {
         return null;
      }
      
      public function set selectedItem(o:*) : void
      {
      }
      
      public function get slotWidth() : uint
      {
         return 0;
      }
      
      public function set slotWidth(value:uint) : void
      {
      }
      
      public function get slotHeight() : uint
      {
         return 0;
      }
      
      public function set slotHeight(value:uint) : void
      {
      }
      
      public function get slotByRow() : uint
      {
         return 0;
      }
      
      public function get slotByCol() : uint
      {
         return 0;
      }
      
      public function get verticalScrollValue() : int
      {
         return 0;
      }
      
      public function set verticalScrollValue(value:int) : void
      {
      }
      
      public function get verticalScrollSpeed() : Number
      {
         return 0;
      }
      
      public function get horizontalScrollSpeed() : Number
      {
         return 0;
      }
      
      public function set verticalScrollSpeed(speed:Number) : void
      {
      }
      
      public function set horizontalScrollSpeed(speed:Number) : void
      {
      }
      
      public function get hiddenRow() : uint
      {
         return 0;
      }
      
      public function get hiddenCol() : uint
      {
         return 0;
      }
      
      public function set hiddenRow(v:uint) : void
      {
      }
      
      public function set hiddenCol(v:uint) : void
      {
      }
      
      public function set slotByRow(v:uint) : void
      {
      }
      
      public function set totalSlotByRow(v:int) : void
      {
      }
      
      public function set ignoreConfigVar(v:Boolean) : void
      {
      }
      
      public function set avaibleSpaceX(v:Number) : void
      {
      }
      
      public function set avaibleSpaceY(v:Number) : void
      {
      }
      
      public function set scrollBarHYOffset(v:Number) : void
      {
      }
      
      public function set scrollBarHXOffset(v:Number) : void
      {
      }
      
      public function get keyboardIndexHandler() : Function
      {
         return new Function();
      }
      
      public function set keyboardIndexHandler(v:Function) : void
      {
      }
      
      public function get silent() : Boolean
      {
         return new Boolean();
      }
      
      public function set silent(v:Boolean) : void
      {
      }
      
      public function renderModificator(childs:Array, accessKey:Object) : Array
      {
         return null;
      }
      
      public function moveToPage(page:uint) : void
      {
      }
      
      public function updateItem(index:uint) : void
      {
      }
      
      public function updateItems() : void
      {
      }
      
      public function get selectedSlot() : DisplayObject
      {
         return null;
      }
      
      public function get slots() : Array
      {
         return null;
      }
      
      public function removeItems() : void
      {
      }
      
      public function indexIsInvisibleSlot(index:uint) : Boolean
      {
         return false;
      }
      
      public function moveTo(index:uint, force:Boolean = false) : void
      {
      }
      
      public function getIndex() : uint
      {
         return 0;
      }
      
      public function sortOn(col:String, options:int = 0) : void
      {
      }
      
      public function sortBy(sortFunction:Function) : void
      {
      }
      
      public function getItemIndex(item:*) : int
      {
         return 0;
      }
   }
}

