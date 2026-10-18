package com.ankamagames.berilia.components
{
   public class TextArea extends Label
   {
      
      public function TextArea()
      {
         super();
      }
      
      public function get resetScroll() : Boolean
      {
         return false;
      }
      
      public function set resetScroll(b:Boolean) : void
      {
      }
      
      public function get scrollBarValue() : int
      {
         return 0;
      }
      
      public function set scrollBarValue(v:int) : void
      {
      }
      
      public function get scrollBarY() : int
      {
         return 0;
      }
      
      public function set scrollBarY(pY:int) : void
      {
      }
      
      public function get scrollBarX() : int
      {
         return 0;
      }
      
      public function set scrollBarX(pX:int) : void
      {
      }
      
      public function get scrollBottomMargin() : int
      {
         return 0;
      }
      
      public function set scrollBottomMargin(value:int) : void
      {
      }
      
      public function get scrollTopMargin() : int
      {
         return 0;
      }
      
      public function set scrollTopMargin(value:int) : void
      {
      }
      
      public function set scrollCss(sUrl:Object) : void
      {
      }
      
      public function get scrollCss() : Object
      {
         return null;
      }
      
      public function set scrollPos(nValue:int) : void
      {
      }
      
      public function get hideScroll() : Boolean
      {
         return false;
      }
      
      public function set hideScroll(hideScroll:Boolean) : void
      {
      }
      
      public function hideScrollBar() : void
      {
      }
   }
}

