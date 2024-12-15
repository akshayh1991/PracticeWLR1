namespace SecMan.BL.Common
{
    public enum Languages
    {
        en,
        fr,
        de,
        zh,
        es,
        enGB,
        it,
        nl,
        pt,
        ko,
        ru,
        ja
    }



    public enum Domain
    {
        local,
        ad
    }


    public enum EventSeverity
    {
        Debug,
        Info,
        Warning,
        Error
    }


    public enum JsonEntities
    {
        User,
        Role,
        SystemFeature,
        Device
    }


    public enum SystemPolicyTypes
    {
        Email,
        Int,
        Bool,
        String,
        IP
    }
}
