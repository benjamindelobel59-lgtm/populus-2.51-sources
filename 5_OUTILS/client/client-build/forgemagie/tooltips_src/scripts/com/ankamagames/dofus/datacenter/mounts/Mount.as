package com.ankamagames.dofus.datacenter.mounts
{
   import com.ankamagames.dofus.datacenter.effects.EffectInstance;
   
   public class Mount
   {
      
      public function Mount()
      {
         super();
      }
      
      public function get id() : uint
      {
         return new uint();
      }
      
      public function get familyId() : uint
      {
         return new uint();
      }
      
      public function get nameId() : uint
      {
         return new uint();
      }
      
      public function get look() : String
      {
         return new String();
      }
      
      public function get certificateId() : uint
      {
         return new uint();
      }
      
      public function get effects() : Vector.<EffectInstance>
      {
         return new Vector.<EffectInstance>();
      }
      
      public function get name() : String
      {
         return null;
      }
   }
}

