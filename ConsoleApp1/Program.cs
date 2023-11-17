using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml.Linq;

//Создаётся счёт и выводится на экран
Bill DelayedBill = new Bill(125, 10, 25, 2);
Console.WriteLine(DelayedBill);

//Сериализуется в JSON-формате
Console.WriteLine("Write Bill to file...");
DelayedBill.Serialize();

//Десериализуется из JSON-формата и выводится на экран
Console.WriteLine("Read Bill from file and print");
Bill? ReadedBill = Bill.Deserialize();
Console.WriteLine(ReadedBill);

//Статическое поле, которое отвечает за полную сериализацию заменяется на false и процедура повторяется
//Для наглядности создаются новые счета и новые файлы
Bill.SerializeCalculated = false;
DelayedBill.Serialize("partialBill.json");
Bill? ReadedPartialBill = Bill.Deserialize("partialBill.json");
//Вычисляемые поля рассчитываются при преобразовании в строку, поэтому просто распечатав разницы не увидеть.
//Поэтому вывожу только пустые поля, которые у оригинала были заполнеными
Console.WriteLine("Not writed data from partial serialized Bill (Bill.PaymentWithPenalty): " + ReadedPartialBill.PaymentWithPenalty);



[Serializable]
[DataContract]
public class Bill
{
    [DataMember]
    public double PayPerDay { get; set; }
    [DataMember]
    public int DaysCount { get; set; }
    [DataMember]
    public double PenaltyPerDay { get; set; }
    [DataMember]
    public int DelayPayment { get; set; }
    [DataMember]
    protected double PaymentWithoutPenalty { get; set; } //вычисляемое поле
    [DataMember]
    protected double PaymentPenalty { get; set; }        //вычисляемое поле
    [DataMember]
    public double PaymentWithPenalty { get; set; }    //вычисляемое поле

    public Bill(double payPerDay, int daysCount, double penaltyPerDay, int delayPayment = 0)
    {
        PayPerDay = payPerDay;
        DaysCount = daysCount;
        PenaltyPerDay = penaltyPerDay;
        DelayPayment = delayPayment;
    }

    public static bool SerializeCalculated { get; set; } = true;
    protected void Calculate()
    {
        PaymentWithoutPenalty = PayPerDay * DaysCount;
        PaymentPenalty = PenaltyPerDay * DelayPayment;
        PaymentWithPenalty = PaymentWithoutPenalty + PaymentPenalty;
    }

    public override string ToString()
    {
        StringBuilder temp = new StringBuilder(255);
        temp.Append($"Pay per day is\t\t${PayPerDay}\n");
        temp.Append($"Days count is\t\t{DaysCount}\n");
        this.Calculate();
        if (DelayPayment > 0)               //если есть просрочка, то выводится полный счёт с пенёй
        {
            temp.Append($"Penalty delay (a day)\t${PenaltyPerDay}\n");
            temp.Append($"Delay of payment (days)\t${DelayPayment}\n");
            temp.Append($"Penalty\t\t\t${PaymentPenalty}\n");
            temp.Append($"___________________________\n");
            temp.Append($"Final bill\t\t${PaymentWithPenalty}\n");
        }
        else                                //иначе итоговый чек пеню не выводит
        {
            temp.Append($"___________________________\n");
            temp.Append($"Final bill\t\t${PaymentWithoutPenalty}\n");
        }
        return temp.ToString();
    }

    public string Serialize(string Path = "bill.json")
    {
        DataContractJsonSerializer BillSaver = new DataContractJsonSerializer(typeof(Bill));
        using (FileStream file = new FileStream(Path, FileMode.Create))
        {

            if (SerializeCalculated)    //если статическое поле true, то сериализуется весь объект
            {
                BillSaver.WriteObject(file, this);
                return this.ToString();
            }

            //если статическое поле false, то сериализуется резанная копия объекта
            Bill temp = new Bill(this.PayPerDay, this.DaysCount, this.PenaltyPerDay, this.DelayPayment);
            BillSaver.WriteObject(file, temp);
            return temp.ToString();
        }
    }

    public static Bill? Deserialize(string Path = "bill.json")
    {
        using (FileStream file = new FileStream(Path, FileMode.Open))
        {
            DataContractJsonSerializer BillDeserializer = new DataContractJsonSerializer(typeof(Bill));
            return (Bill?)BillDeserializer.ReadObject(file);
        }
    }





}

