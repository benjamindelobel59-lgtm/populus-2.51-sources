package com.ankamagames.dofus.datacenter.effects.instances
{
   import com.ankamagames.dofus.datacenter.effects.EffectInstance;
   
   public class EffectInstanceMount extends EffectInstance
   {
      
      public function EffectInstanceMount()
      {
         super();
      }
      
      public function get id() : Number
      {
         return new Number();
      }
      
      public function get expirationDate() : Number
      {
         return new Number();
      }
      
      public function get model() : uint
      {
         return new uint();
      }
      
      public function get name() : String
      {
         return new String();
      }
      
      public function get owner() : String
      {
         return new String();
      }
      
      public function get level() : uint
      {
         return new uint();
      }
      
      public function get sex() : Boolean
      {
         return new Boolean();
      }
      
      public function get isRideable() : Boolean
      {
         return new Boolean();
      }
      
      public function get isFeconded() : Boolean
      {
         return new Boolean();
      }
      
      public function get isFecondationReady() : Boolean
      {
         return new Boolean();
      }
      
      public function get reproductionCount() : int
      {
         return new int();
      }
      
      public function get reproductionCountMax() : uint
      {
         return new uint();
      }
      
      public function get effects() : Vector.<EffectInstanceInteger>
      {
         return new Vector.<EffectInstanceInteger>();
      }
      
      public function get capacities() : Vector.<uint>
      {
         return new Vector.<uint>();
      }
   }
}

