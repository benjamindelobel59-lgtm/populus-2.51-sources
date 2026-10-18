package com.ankamagames.dofus.uiApi
{
   import com.ankamagames.berilia.types.shortcut.Bind;
   import com.ankamagames.berilia.types.shortcut.Shortcut;
   
   public class BindsApi
   {
      
      public function BindsApi()
      {
         super();
      }
      
      public function destroy() : void
      {
      }
      
      public function getBindList() : Array
      {
         return null;
      }
      
      public function getShortcut() : Array
      {
         return null;
      }
      
      public function getShortcutBind(shortcutName:String, returnDisabled:Boolean = false) : Bind
      {
         return null;
      }
      
      public function getShortcutsFromKeyCode(pKeyCode:uint) : Shortcut
      {
         return null;
      }
      
      public function setShortcutBind(targetedShorcut:String, key:String, alt:Boolean, ctrl:Boolean, shift:Boolean) : void
      {
      }
      
      public function removeShortcutBind(targetedBind:String) : void
      {
      }
      
      public function getShortcutBindStr(shortcutName:String, returnDisabled:Boolean = false) : String
      {
         return null;
      }
      
      public function resetAllBinds() : void
      {
      }
      
      public function availableKeyboards() : Array
      {
         return null;
      }
      
      public function changeKeyboard(locale:String) : void
      {
      }
      
      public function getCurrentLocale() : String
      {
         return null;
      }
      
      public function bindIsRegister(bind:Bind) : Boolean
      {
         return false;
      }
      
      public function bindIsPermanent(bind:Bind) : Boolean
      {
         return false;
      }
      
      public function bindIsDisabled(bind:Bind) : Boolean
      {
         return false;
      }
      
      public function setBindDisabled(bind:Bind, disabled:Boolean) : void
      {
      }
      
      public function getRegisteredBind(bind:Bind) : Bind
      {
         return null;
      }
      
      public function getShortcutByName(name:String) : Shortcut
      {
         return null;
      }
      
      public function setShortcutEnabled(enabled:Boolean) : void
      {
      }
      
      public function getIsShortcutEnabled() : Boolean
      {
         return false;
      }
      
      public function disableShortcut(name:String, val:Boolean) : void
      {
      }
      
      public function enableShortcutKey(keyCode:uint, charCode:uint, enabled:Boolean) : void
      {
      }
   }
}

