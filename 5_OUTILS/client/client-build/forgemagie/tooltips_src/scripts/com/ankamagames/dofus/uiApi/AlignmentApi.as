package com.ankamagames.dofus.uiApi
{
   import com.ankamagames.dofus.datacenter.alignments.AlignmentBalance;
   import com.ankamagames.dofus.datacenter.alignments.AlignmentEffect;
   import com.ankamagames.dofus.datacenter.alignments.AlignmentGift;
   import com.ankamagames.dofus.datacenter.alignments.AlignmentOrder;
   import com.ankamagames.dofus.datacenter.alignments.AlignmentRank;
   import com.ankamagames.dofus.datacenter.alignments.AlignmentRankJntGift;
   import com.ankamagames.dofus.datacenter.alignments.AlignmentSide;
   
   public class AlignmentApi
   {
      
      public function AlignmentApi()
      {
         super();
      }
      
      public function destroy() : void
      {
      }
      
      public function getBalance(balanceId:uint) : AlignmentBalance
      {
         return null;
      }
      
      public function getBalances() : Array
      {
         return null;
      }
      
      public function getEffect(effectId:uint) : AlignmentEffect
      {
         return null;
      }
      
      public function getGift(giftId:uint) : AlignmentGift
      {
         return null;
      }
      
      public function getGifts() : Array
      {
         return null;
      }
      
      public function getRankGifts(rankId:uint) : AlignmentRankJntGift
      {
         return null;
      }
      
      public function getGiftEffect(giftId:uint) : AlignmentEffect
      {
         return null;
      }
      
      public function getOrder(orderId:uint) : AlignmentOrder
      {
         return null;
      }
      
      public function getOrders() : Array
      {
         return null;
      }
      
      public function getRank(rankId:uint) : AlignmentRank
      {
         return null;
      }
      
      public function getRanks() : Array
      {
         return null;
      }
      
      public function getRankOrder(rankId:uint) : AlignmentOrder
      {
         return null;
      }
      
      public function getOrderRanks(orderId:uint) : Array
      {
         return null;
      }
      
      public function getSide(sideId:uint) : AlignmentSide
      {
         return null;
      }
      
      public function getOrderSide(orderId:uint) : AlignmentSide
      {
         return null;
      }
      
      public function getSideOrders(sideId:uint) : Array
      {
         return null;
      }
      
      public function getTitleName(sideId:uint, grade:int) : String
      {
         return null;
      }
      
      public function getTitleShortName(sideId:uint, grade:int) : String
      {
         return null;
      }
      
      public function getPlayerRank() : int
      {
         return 0;
      }
      
      public function getAlliancesOnTheHill() : Object
      {
         return null;
      }
   }
}

