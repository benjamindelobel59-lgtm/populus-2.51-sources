package com.ankamagames.dofus.uiApi
{
   public class SoundApi
   {
      
      public function SoundApi()
      {
         super();
      }
      
      public function destroy() : void
      {
      }
      
      public function activateSounds(pActivate:Boolean) : void
      {
      }
      
      public function soundsAreActivated() : Boolean
      {
         return false;
      }
      
      public function setBusVolume(pAudioBusId:uint, pVolume:uint) : void
      {
      }
      
      public function playSoundById(pSoundId:String) : void
      {
      }
      
      public function playStopablesoundById(pSoundId:String) : void
      {
      }
      
      public function stopStopablesoundById(pSoundId:String) : void
      {
      }
      
      public function fadeBusVolume(pBusId:uint, pFade:Number, pFadeTime:uint) : void
      {
      }
      
      public function playSound(pSound:uint) : void
      {
      }
      
      public function playLookSound(pLook:String, pSoundType:uint) : void
      {
      }
      
      public function playIntroMusic() : void
      {
      }
      
      public function switchIntroMusic(pFirstHarmonic:Boolean = true) : void
      {
      }
      
      public function stopIntroMusic() : void
      {
      }
      
      public function playSoundAtTurnStart() : Boolean
      {
         return false;
      }
      
      public function playSoundForGuildMessage() : Boolean
      {
         return false;
      }
      
      public function isSoundMainClient() : Boolean
      {
         return false;
      }
   }
}

