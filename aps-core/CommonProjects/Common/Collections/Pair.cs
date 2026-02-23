namespace PT.Common;

public class Pair<Ty1, Ty2>
{
    public Pair(Ty1 aValue1, Ty2 aValue2)
    {
        value1 = aValue1;
        value2 = aValue2;
    }

    public Ty1 value1;
    public Ty2 value2;

    public override string ToString()
    {
        return value1 + " " + value2;
    }
}