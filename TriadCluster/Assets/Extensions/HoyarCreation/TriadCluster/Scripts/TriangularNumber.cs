namespace HoyarCreation.TriadCluster
{
    public static class TriangularNumber
    {
        private static int[] s_NumbersCache = new int[k_MaxTermCount];
        private const int k_MaxTermCount = 1000;

        private static bool s_IsCacheInited = false;

        public static int[] GetCachedTerm0ToTerm999Numbers()
        {
            InitCache();
            return s_NumbersCache;
        }

        public static int GetNumber(int term)
        {
            if (term < 0 || term >= k_MaxTermCount) return -1;
            InitCache();
            return s_NumbersCache[term];
        }

        private static void InitCache()
        {
            if (s_IsCacheInited) return;
            for (int i = 0; i < s_NumbersCache.Length; i++)
            {
                if (s_NumbersCache[i] == default)
                {
                    s_NumbersCache[i] = CalcNumberByTerm(i);
                }
            }

            s_IsCacheInited = true;

            static int CalcNumberByTerm(int term)
            {
                return term * (term + 1) / 2;
            }
        }
    }
}