namespace Umacalc.Models;

public static class CalendarHelper
{
    private static readonly string[] Months =
    {
        "Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
    };

    public static string GetTurnDateString(int turnNumber)
    {
        if (turnNumber < 1) turnNumber = 1;

        if (turnNumber <= 24)
        {
            // Junior Year (1-24)
            int index = turnNumber - 1;
            string month = Months[index / 2];
            string half = (index % 2 == 0) ? "Early" : "Late";
            string note = "";
            if (turnNumber == 1) note = " (Pre-Debut)";
            else if (turnNumber == 12) note = " (Debut Window)";
            return $"Junior {month} {half}{note}";
        }
        else if (turnNumber <= 48)
        {
            // Classic Year (25-48)
            int index = turnNumber - 25;
            string month = Months[index / 2];
            string half = (index % 2 == 0) ? "Early" : "Late";
            string note = "";
            // Summer camp is July Early to August Late (index 12, 13, 14, 15) -> turns 37, 38, 39, 40
            if (turnNumber >= 37 && turnNumber <= 40)
            {
                note = " ⛺ [Summer Camp]";
            }
            else if (turnNumber == 31) note = " (Satsuki/Oka Sho)";
            else if (turnNumber == 34) note = " (Tokyo Yushun/Oaks)";
            else if (turnNumber == 44) note = " (Kikka/Shuka Sho)";
            else if (turnNumber == 48) note = " (Arima Kinen)";

            return $"Classic {month} {half}{note}";
        }
        else if (turnNumber <= 72)
        {
            // Senior Year (49-72)
            int index = turnNumber - 49;
            string month = Months[index / 2];
            string half = (index % 2 == 0) ? "Early" : "Late";
            string note = "";
            // Summer camp turns 61, 62, 63, 64
            if (turnNumber >= 61 && turnNumber <= 64)
            {
                note = " ⛺ [Summer Camp]";
            }
            else if (turnNumber == 56) note = " (Tenno Sho Spring)";
            else if (turnNumber == 60) note = " (Takarazuka Kinen)";
            else if (turnNumber == 68) note = " (Tenno Sho Autumn)";
            else if (turnNumber == 70) note = " (Japan Cup)";
            else if (turnNumber == 72) note = " (Arima Kinen)";

            return $"Senior {month} {half}{note}";
        }
        else
        {
            int finaleRound = turnNumber - 72;
            return finaleRound switch
            {
                1 => "Finale - Round 1 (Qualifiers)",
                2 => "Finale - Round 2 (Semifinals)",
                3 => "Finale - Round 3 (Finals)",
                _ => $"Finale - Round {finaleRound}"
            };
        }
    }

    public static string GetPeriodCategory(int turnNumber)
    {
        if (turnNumber <= 24) return "Junior Year";
        if (turnNumber <= 48) return "Classic Year";
        if (turnNumber <= 72) return "Senior Year";
        return "Finale";
    }
}
