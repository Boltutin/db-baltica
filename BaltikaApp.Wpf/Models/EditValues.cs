namespace BaltikaApp.Data
{
    /// <summary>Данные судна, собранные в диалоге редактирования.</summary>
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

    /// <summary>Данные рейса, собранные в диалоге редактирования.</summary>
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

    /// <summary>Данные груза, собранные в диалоге редактирования.</summary>
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

    /// <summary>Данные отправителя, собранные в диалоге редактирования.</summary>
    public struct SenderEditValues
    {
        public string SenderName;
        public string Inn;
        public int BankId;
        public int AddressId;
    }

    /// <summary>Данные получателя, собранные в диалоге редактирования.</summary>
    public struct ConsigneeEditValues
    {
        public string ConsigneeName;
        public string Inn;
        public int BankId;
        public int AddressId;
    }
}
