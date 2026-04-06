namespace BaltikaApp.Data
{
    /// <summary>Данные судна из диалога редактирования (таблица <c>ships</c>).</summary>
    public struct ShipEditValues
    {
        public string RegNumber;
        public string Name;
        public int CaptainId;
        public int TypeId;
        public int DockyardId;
        public int Capacity;
        public int YearBuilt;
        public decimal CustomsValue;
        public int HomePortId;
    }

    /// <summary>Данные рейса из диалога редактирования (таблица <c>shipments</c>).</summary>
    public struct ShipmentEditValues
    {
        public int ShipId;
        public int OriginPortId;
        public int DestinationPortId;
        public DateTime DepartureDate;
        public DateTime? ArriveDate;
        public decimal? CustomsValue;
        public bool CustomClearance;
    }

    /// <summary>Данные груза из диалога редактирования (таблица <c>cargo</c>).</summary>
    public struct CargoEditValues
    {
        public int CargoId;
        public int ShipmentId;
        public int SenderId;
        public int ConsigneeId;
        public int CargoNumber;
        public string CargoName;
        public int UnitId;
        public decimal DeclaredValue;
        public decimal InsuredValue;
        public decimal CustomValue;
        public decimal Quantity;
        public string Comment;
    }

    /// <summary>Данные отправителя из диалога редактирования (таблица <c>senders</c>).</summary>
    public struct SenderEditValues
    {
        public string SenderName;
        public string Inn;
        public int BankId;
        public int AddressId;
    }

    /// <summary>Данные получателя из диалога редактирования (таблица <c>consignees</c>).</summary>
    public struct ConsigneeEditValues
    {
        public string ConsigneeName;
        public string Inn;
        public int BankId;
        public int AddressId;
    }
}
