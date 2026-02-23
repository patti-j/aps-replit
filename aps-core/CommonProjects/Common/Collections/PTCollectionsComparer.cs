namespace PT.Common.Collections;

public interface IPTCollectionsComparer<Ty>
{
    bool LessThan(Ty a_n1, Ty a_n2);

    bool LessThanOrEqual(Ty a_n1, Ty a_n2);

    bool GreaterThan(Ty a_n1, Ty a_n2);

    bool GreaterThanOrEqual(Ty a_n1, Ty a_n2);

    bool EqualTo(Ty a_n1, Ty a_n2);

    bool NotEqualTo(Ty a_n1, Ty a_n2);
}