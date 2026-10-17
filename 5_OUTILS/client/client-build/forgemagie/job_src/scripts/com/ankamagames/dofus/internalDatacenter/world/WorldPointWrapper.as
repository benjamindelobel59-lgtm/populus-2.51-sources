package com.ankamagames.dofus.internalDatacenter.world
{
   import com.ankamagames.jerakine.types.positions.WorldPoint;
   
   public class WorldPointWrapper extends WorldPoint
   {
      
      public function WorldPointWrapper()
      {
         super();
      }
      
      public function get outdoorMapId() : Number
      {
         return new Number();
      }
      
      public function get outdoorX() : int
      {
         return 0;
      }
      
      public function get outdoorY() : int
      {
         return 0;
      }
      
      public function get topNeighbourId() : Number
      {
         return 0;
      }
      
      public function get bottomNeighbourId() : Number
      {
         return 0;
      }
      
      public function get leftNeighbourId() : Number
      {
         return 0;
      }
      
      public function get rightNeighbourId() : Number
      {
         return 0;
      }
   }
}

