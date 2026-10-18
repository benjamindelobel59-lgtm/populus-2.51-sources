package com.ankamagames.berilia.components
{
   public class TextAreaInput extends TextArea
   {
      
      public function TextAreaInput()
      {
         super();
      }
      
      public function get imeActive() : Boolean
      {
         return new Boolean();
      }
      
      public function set imeActive(v:Boolean) : void
      {
      }
      
      public function get focusEventHandlerPriority() : Boolean
      {
         return new Boolean();
      }
      
      public function set focusEventHandlerPriority(v:Boolean) : void
      {
      }
      
      public function get lastTextOnInput() : String
      {
         return null;
      }
      
      public function get maxChars() : uint
      {
         return 0;
      }
      
      public function set maxChars(nValue:uint) : void
      {
      }
      
      public function get restrictChars() : String
      {
         return null;
      }
      
      public function set restrictChars(sValue:String) : void
      {
      }
      
      public function get haveFocus() : Boolean
      {
         return false;
      }
      
      public function blur() : void
      {
      }
      
      public function setSelection(start:int, end:int) : void
      {
      }
      
      public function addHyperLinkCode(pHyperlinkCode:String) : void
      {
      }
      
      public function getHyperLinkCodes() : Object
      {
         return null;
      }
   }
}

