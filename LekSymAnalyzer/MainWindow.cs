using System;
using System.Collections.Generic;
using System.Windows.Forms;
enum st { H, // Начало программы
    I, // идентификатор
    N, // число(целая часть)
    ND, // число(дробная часть)
    NS, // знак порядка
    NP, // порядок
    O, // ограничитель
    C, // комментарий
    L, // меньше, неравно, меньшеравно
    G, // больше
    S, // равенство
    DT, // конец программы
    ER, // ошибка
    V, // конец строки
    SS, // составной оператор
    SK // оператор скобки
}
namespace LekSymAnalyzer {
    public partial class MainWindow : Form {
        public MainWindow() {
            InitializeComponent();
        }
        char[] chr;
        int lid = 0, len = 0;
        public struct lex {
            public int code;
            public int chr;
            public int line;
            public int linech;
            public int wher;
            public VarType type;
            public lex(int c, int s, int ln, int lnc, int wh) {
                code = c;
                chr = s;
                line = ln;
                linech = lnc;
                wher = wh;
                type = VarType.none;
            }
            public string get() {
                return lsts[code - 1][chr - 1];
            }
        }
        List<lex> lexems = new List<lex>();
        public bool strchr(string originalString, char charToSearch) {
            int found = originalString.IndexOf(charToSearch);
            return found > -1 ? true : false;
        }
        int line = 0;
        int linech = 0;
        public char gc() {
            char c1 = chr[lid++];
            if (c1 == '\n') {
                line++;
                linech = 0;
            } else
                linech++;
            return c1;
        }
        public void write(char st) {
            richTextBox2.Text += st;
        }
        public void write(string st) {
            richTextBox2.Text += st;
        }
        public void tout(int i, int s) {
            lexems.Add(new lex(i, s, line, linech, lid));
            write("(" + i + "," + s + ") ");
        }
        public bool isletter(char ch) {
            if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z'))
                return true;
            return false;
        }
        public bool isdigit(char ch) {
            if (ch >= '0' && ch <= '9')
                return true;
            return false;
        }
        string[] delimiters = { "(", ")", ",", ".", ";", ":", "\n", "<", ">", "=",
            "<>", "<=", ">=", "+", "-", "*", "/", "{", "}", "[", "]", "<>" };
        string[] keywords = { "program", "var", "begin", "end", "integer", "real",
            "bool", "read", "write", "if", "then", "else", "while", "do", "true",
            "false", "to", "or", "and", "not", "as", "for" };
        static List<string>[] lsts;
        DataGridView[] dg;
        List<string> keys = new List<string>();
        List<string> delimit = new List<string>();
        List<string> identifiers = new List<string>();
        List<string> numbers = new List<string>();
        int keymax = 0, delmax = 0;
        st state; int MaxIdent = 8;
        string errcode = string.Empty; object[] errobj;
        public void err(string code, params object[] obj) {
            errcode = code;
            errobj = obj;
            throw new Exception();
        }
        string[] efall;
        public void Init() {
            int ln = keywords.Length, max = 0 ;
            for (int i = 0; i < ln; i++)
                if (keywords.Length > max)
                    max = keywords.Length;
            keymax = max;
            ln = delimiters.Length;
            for (int i = 0; i < ln; i++)
                if (delimiters.Length > max)
                    max = delimiters.Length;
            delmax = max;
            lsts = new List<string>[4] { keys, delimit, numbers, identifiers };
            dg = new DataGridView[4] { dataGridView1, dataGridView2, dataGridView3, dataGridView4 };
            for (int i = 0; i < 4; i++) {
                dg[i].RowsAdded += DataGridView_RowsAdded;
                dg[i].ColumnHeadersVisible = false;
                dg[i].ColumnCount = 1;
                dg[i].Columns[0].Name = "value";
                dg[i].TopLeftHeaderCell.Value = "";
                dg[i].RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
                dg[i].AutoResizeRows();
                dg[i].RowTemplate.Height = 16;
                dg[i].RowTemplate.Resizable = DataGridViewTriState.False;
                dg[i].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            dataGridView3.ColumnCount = 2;
            dataGridView3.Columns[1].Width = 22;
            dataGridView2.ColumnCount = 2;
            dataGridView2.Columns[1].Width = 22;
            int len = exp.Length + operand.Length;
            string[] newArray = new string[exp.Length + operand.Length + 1];
            Array.Copy(exp, newArray, exp.Length);
            Array.Copy(operand, 0, newArray, exp.Length, operand.Length);
            newArray[len] = ")";
            efall = newArray;
            Types.Add("integer", new List<lex>());
            Types.Add("real", new List<lex>());
            Types.Add("bool", new List<lex>());
        }
        private void DataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e) {
            DataGridView grid = (DataGridView) sender;
            int cnt = grid.RowCount;
            for (int i = 0; i < cnt; i++)
                grid.Rows[i].HeaderCell.Value = (i+1).ToString();
        }
        public void Start() {
            state = st.H;
            lexems.Clear();
            richTextBox2.Clear();
            lid = 0;
            errcode = string.Empty;
            errobj = null;
            for (int i = 0; i < 4; i++) {
                dg[i].Rows.Clear();
                lsts[i].Clear();
            }
            line = 0;
            linech = 0;
        }
        public int ident(string str, int tbl) {
            List<string> tab = identifiers;
            DataGridView grid = dataGridView3;
            switch (tbl) {
                case 1: {
                        tab = keys;
                        grid = dataGridView1;
                        break;
                    }
                case 2: {
                        tab = delimit;
                        grid = dataGridView4;
                        break;
                    }
                case 3: {
                        tab = numbers;
                        grid = dataGridView2;
                        break;
                    }
                case 4: {
                        break;
                    }
                default: {
                        err(string.Empty);
                        break;
                    }
            }
            int index = tab.IndexOf(str);
            if (index >= 0)
                return ++index;
            else {
                tab.Add(str);
                if (str == "\n")
                    str = "\\n";
                index = grid.Rows.Add(str);
                return ++index;
            }
        }
        int insid = 0;
        public bool look(string str, int tbl) {
            string[] tab = keywords;
            DataGridView grid = dataGridView1;
            int max = keymax;
            switch (tbl) {
                case 1: {
                        break;
                    }
                case 2: {
                        tab = delimiters;
                        max = delmax;
                        grid = dataGridView4;
                        break;
                    }
                default: {
                        err(string.Empty);
                        break;
                    }
            }
            if (str.Length > max)
                return false;
            int ln = tab.Length;
            for (int i = 0; i < ln; i++)
                if (tab[i] == str) {
                    insid = ident(str, tbl);
                    return true;
                }
            return false;
        }
        public void LexScan(string txt) {
            txt += ' ';
            chr = txt.ToCharArray();
            len = chr.Length;
            if (len == 0) return;
            Start();
            string cur = string.Empty;
            try {
                for (; lid < len;) {
                    char ch = gc();
                    if (isletter(ch)) {
                        state = st.I;
                        cur = string.Empty + ch;
                        for (; lid < len;) {
                            ch = gc();
                            if (isletter(ch) || isdigit(ch)) {
                                cur += ch;
                                if (cur.Length > MaxIdent)
                                    err("Идентификатор слишком длинный.");
                            } else {
                                lid--;
                                break;
                            }
                        }
                        if (look(cur, 1))
                            tout(1, insid);
                        else
                            tout(4, ident(cur, 4));
                    } else if (isdigit(ch) || ch == '.') {
                        state = st.N;
                        cur = string.Empty;
                        if (ch != '.') {
                            cur += ch;
                            for (; lid < len;) {
                                ch = gc();
                                if (isdigit(ch) || strchr("ABCDEFabcdef", ch))
                                    cur += ch;
                                else break;
                            }
                            if (strchr("HhDdOoBb", ch)) {
                                cur += ch;
                                continue;
                            }
                        } else {
                            ch = gc();
                            if (!isdigit(ch)) {
                                tout(2, ident(".", 2));
                                continue;
                            }
                            lid--;
                            ch = '.';
                        }
                        if (ch == '.') {
                            state = st.ND;
                            cur += ch;
                            for (; lid < len;) {
                                ch = gc();
                                if (isdigit(ch))
                                    cur += ch;
                                else break;
                            }
                            if (ch == 'e' || ch == 'E') {
                                cur += ch;
                                ch = gc();
                                state = st.NS;
                                if (ch == '+' || ch == '-')
                                    cur += ch;
                                state = st.NP;
                                for (; lid < len;) {
                                    ch = gc();
                                    if (isdigit(ch))
                                        cur += ch;
                                    else {
                                        if (isletter(ch)) {
                                            cur += ch;
                                            err("Неправильный порядок в числе {0}.", ch);
                                        }
                                        lid--;
                                        break;
                                    }
                                }
                            } else lid--;
                        } else if ((ch == '+' || ch == '-' || isdigit(ch)) && (chr[lid - 2] == 'e' || chr[lid - 2] == 'E')) {
                            state = st.NP;
                            cur += ch;
                            for (; lid < len;) {
                                ch = gc();
                                if (isdigit(ch))
                                    cur += ch;
                                else {
                                    lid--;
                                    break;
                                }
                            }
                            ch = gc();
                            if (strchr("HhDdOoBb", ch))
                                cur += ch;
                            else {
                                if (isletter(ch) || isdigit(ch)) {
                                    cur += ch;
                                    err("Неверное число {0}.", cur);
                                }
                                lid--;
                            }
                        } else lid--;
                        tout(3, ident(cur, 3));
                    } else if (ch == '{') {
                        state = st.C;
                        for (; lid < len;)
                            if (gc() == '}')
                                break;
                    } else if (ch == '<') {
                        state = st.L;
                        ch = gc();
                        if (ch == '=' || ch == '>')
                            cur = "<" + ch;
                        else cur = "<";
                        tout(2, ident(cur, 2));
                    } else if (ch == '>') {
                        state = st.G;
                        ch = gc();
                        if (ch == '=')
                            cur = ">=";
                        else
                            cur = ">";
                        tout(2, ident(cur, 2));
                    } else if (ch == '=') {
                        state = st.S;
                        tout(2, ident("=", 2));
                    } else {
                        cur = string.Empty + ch;
                        if (look(cur, 2))
                            tout(2, insid);
                    }
                }
            } catch(Exception ex) {
                string s = string.Empty;
                if (errcode == string.Empty)
                    s += ex.ToString();
                else
                    s += (errobj.Length>0 ? String.Format(errcode,errobj) : errcode) +
                        "\nЛиния " + line + " в позиции " + linech + ".";
                MessageBox.Show(s, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Form1_Load(object sender, EventArgs e) {
            Init();
        }
        int clex = 0;
        private lex GetLex(int lx=1) {
            clex += lx;
            if (clex >= lexems.Count)
                err("Неожиданный конец строки.");
            return lexems[clex];
        }
        private lex? NextLex(int tbl) {
            int cl = lexems.Count;
            for(int k = clex; k < cl; k++) {
                lex i = lexems[k];
                if (i.chr == tbl) {
                    clex = k;
                    return i;
                }
            }
            return null;
        }
        private lex SkipLex(bool sos = false) {
            if (sos) return GetLex();
            for (int i = clex, cl = lexems.Count; i < cl; i++) {
                lex l = GetLex();
                if (l.code != 2 || l.get() != "\n")
                    return l;
            }
            return lexems[0];
        }
        private bool Exp(string tn, int k) {
            char c = tn[k];
            if (c == 'e' || c == 'E') {
                if ((k + 1) > tn.Length)
                    return false;
                c = tn[k + 1];
                if (c == '+' || c == '-') {
                    if ((k + 2) > tn.Length)
                        return false;
                    c = tn[k + 2];
                    if (isdigit(c))
                        return true;
                } else if(isdigit(c))
                    return true;
            } else if (c == '+' || c == '-') {
                if ((k + 1) > tn.Length)
                    return false;
                c = tn[k + 1];
                if (!isdigit(c))
                    return false;
                c = tn[k - 1];
                if (c == 'e' || c == 'E')
                    return true;
            }
            return false;
        }
        string hexa = "0123456789ABCDEFabcdef", deca = "0123456789", octa = "01234567", bina = "01";
        private void LexVerify() {
            try {
                for(int j = 0, cl = numbers.Count; j < cl; j++ ) {
                    string tni = numbers[j];
                    int sl = tni.Length - 1;
                    char t = tni[sl];
                    int dots = 0;
                    if (t == 'H' || t == 'h') {
                        for (int k = 0; k < sl; k++)
                            if (!strchr(hexa, tni[k]) && !Exp(tni, k))
                                if (tni[k] == '.') { dots++; continue; } else
                                    err("Неправильное шестнадцатеричное число: {0}.", tni);
                    } else if (t == 'O' || t == 'o') {
                        for (int k = 0; k < sl; k++)
                            if (!strchr(octa, tni[k]) && !Exp(tni, k))
                                if (tni[k] == '.') { dots++; continue; } else
                                    err("Неправильное восьимеричное число: {0}.", tni);
                    } else if (t == 'B' || t == 'b') {
                        for (int k = 0; k < sl; k++)
                            if (!strchr(bina, tni[k]) && !Exp(tni, k))
                                if (tni[k] == '.') { dots++; continue; } else
                                    err("Неправильное двоичное число: {0}.", tni);
                    } else if (t == 'D' || t == 'd' || isdigit(t)) {
                        for (int k = 0; k < sl; k++)
                            if (!strchr(deca, tni[k]) && !Exp(tni, k))
                                if (tni[k] == '.') { dots++; continue; } else
                                    err("Неправильное десятичное число: {0}.", tni);
                    } else
                        err("Неправильное число: {0}.", tni);
                    if (dots > 1)
                        err("Неправильное число: {0}. Содержит {1} дробных(-е) частей(и).", tni, dots);
                }
            }catch(Exception ex) {
                string s = string.Empty;
                if (errcode == string.Empty)
                    s += ex.ToString();
                else
                    s += errobj.Length > 0 ? String.Format(errcode, errobj) : errcode;
                MessageBox.Show(s, "Лексическая ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        int wordsz = 0;
        bool _DEBUG = false;
        public void synterr(string er, lex lx, params object[] obj) {
            line = lx.line;
            linech = lx.linech;
            lid = lx.wher;
            string lex = lx.get();
            wordsz = lex.Length;
            err(er + "\nЛексема = '" + (lex=="\n" ? "\\n" : lex) + "'.", obj);
        }
        //1-служ.симв, 2-разд., 3-числа, 4-идент.
        static string[] exp = { "=", ">", "<", "<>", ">=", "<=" };
        static string[] operand = { "*", "/", "+", "-" };
        // FACT
        private bool varv(bool sos, ref int level) {
            lex l = SkipLex(sos);
            if (l.code == 1) {
                string lx = l.get();
                if (lx == "not")
                    return varv(sos, ref level);
                if (lx == "true" || lx == "false")
                    return true;
            } else if (l.code == 2) {
                string lx = l.get();
                if (lx == "(") {
                    level++;
                    return varv(sos, ref level);
                } else if (lx == ")") {
                    if (level > 0)
                        return true;
                } else if (sos && lx == "\n")
                    synterr("Недопустимый перенос строки в составном операторе.", l);
            } else if (l.code == 3 || l.code == 4)
                return true;
            return false;
        }
        // E1
        private bool expr(bool sos, ref int level) {
            lex l = SkipLex();
            if (l.code == 1) {
                string lx = l.get();
                if (lx == "and" || lx == "or")
                    return true;
            } else if (l.code == 2) {
                if (l.get() == ")") {
                    level--;
                    return expr(sos, ref level);
                }
                if (Array.IndexOf(efall, l.get()) >= 0)
                    return true;
            }
            return false;
        }
        // EXP
        private void ifv(bool sos) {
            int level = 0;
            while (true) {
                if (varv(sos, ref level)) {
                    //MessageBox.Show("varv " + lexems[clex].get());
                    if (!expr(sos, ref level)) {
                        clex--;
                        break;
                    }
                    //MessageBox.Show("expr " + lexems[clex].get());
                } else {
                    clex--;
                    break;
                }
            }
            if (level > 0)
                synterr("Отсутствует закрывающая скобка. {0}", GetLex());
            else if (level < 0)
                clex += level;
        }
        // if EXP then OPR else OPR
        private void ifelse(bool sos) {
            ifv(sos);
            lex l = SkipLex(sos);
            if (l.get() == "then")
                Operator(sos);
            else synterr("Неправильный условный оператор.", l);
            l = SkipLex(sos);
            if (l.get() == "else")
                Operator(sos);
            else
                clex--;
        }
        // while EXP do OPR
        private void whileloop(bool sos) {
            ifv(sos);
            lex l = SkipLex(sos);
            if (l.get() == "do")
                Operator(sos);
            else synterr("Неправильный оператор условного цикла.", l);
        }
        // for IND as EXP to EXP do OPR
        private void forloop(bool sos) {
            lex l = SkipLex(sos);
            if (l.code != 4)
                synterr("Неправильное присвоение в теле цикла.", l);
            Prisv(sos);
            l = SkipLex(sos);
            if (l.get() != "to")
                synterr("Неправильный оператор фиксированного цикла.", l);
            int level = 0;
            if (!varv(sos, ref level))
                synterr("Фиксированный цикл не имеет границы.", l);
            l = SkipLex(sos);
            if (l.get() != "do")
                synterr("Фиксированный цикл не окончен.", l);
            Operator(sos);
        }
        // I1
        private void I1(bool sos) {
            bool vr = false;
            while (true) {
                lex l = SkipLex(sos);
                if (l.get() == ",") {
                    if (vr) l = SkipLex(sos);
                    else synterr("Попытка объявить пустой идентификатор.", l);
                } else if (l.code != 4) {
                    clex--;
                    break;
                }
                vr = true;
            }
        }
        // IND AS EXP
        private void Prisv(bool sos) {
            lex l = SkipLex(sos);
            if (l.code == 1 && l.get() == "as") {
                ifv(sos);
            } else
                synterr("Неправильный оператор присвоения.", l);
        }
        // read(I1)
        private void read(bool sos) {
            lex l = SkipLex(sos);
            if (l.code == 2 && l.get() == "(") {
                I1(sos);
                l = SkipLex(sos);
                if (l.code == 2 && l.get() == ")")
                    return;
                else
                    synterr("Отсутствует закрывающая скобка в операторе ввода.", l);
            } else
                synterr("Неправильный оператор ввода.", l);
        }
        // write(E2)
        private void write(bool sos) {
            lex l = SkipLex(sos);
            if (l.code == 2 && l.get() == "(") {
                ifv(sos);
                l = SkipLex(sos);
                if (l.code == 2 && l.get() == ")")
                    return;
                else
                    synterr("Отсутствует закрывающая скобка в операторе вывода.", l);
            } else
                synterr("Отсутствует открывающая скобка в операторе вывода.", l);
        }
        // S1
        private void SosOper() {
            while (true) {
                if (!Operator(true))
                    break;
            }
        }
        // OPR
        private bool Operator(bool sos = false) {
            lex l = SkipLex(sos);
            if (l.code == 1) {
                string lx = l.get();
                if (lx == "if") ifelse(sos);
                else if (lx == "while") whileloop(sos);
                else if (lx == "for") forloop(sos);
                else if (lx == "read") read(sos);
                else if (lx == "write") write(sos);
                else if (!sos && lx == "end") return false;
            } else if (l.code == 2) {
                if (l.get() == "[")
                    SosOper();
                else if (sos) {
                    string lx = l.get();
                    if (lx == ":" || lx == "\n")
                        return true;
                    else if (lx == "]")
                        return false;
                    else
                        synterr("Неверный составной цикл.", l);
                }
            } else if (l.code == 4)
                Prisv(sos);
            else synterr("Неправильное объявление оператора.", l);
            return true;
        }
        private void SyntVerify() {
            try {
                line = 0;
                linech = 0;
                clex = 0;
                lex? i = NextLex(1);
                if (i == null) err("Не найдено служебных слов.");
                if (i.Value.get() != "program")
                    synterr("Программа должна начинаться со служебного слова 'program'.", i.Value);
                // PRO
                lex l = SkipLex();
                if (l.get() != "var") synterr("Отсутствует служебное слово 'var'.", l);
                // D2
                while (true) {
                    l = SkipLex();
                    string lx = l.get();
                    // DIM
                    if (lx == "integer" || lx == "bool" || lx == "real") {
                        // D1
                        while (true) {
                            l = SkipLex();
                            // I1
                            if (l.code != 4)
                                synterr("Неправильный идентификатор '{0}'!", l, l.get());
                            FormIdent(lx, l);
                            l = SkipLex();
                            if (l.code == 2) {
                                string lex = l.get();
                                if (lex == ",")
                                    continue;
                                else if (lex == ";")
                                    break;
                            } else
                                synterr("Незаконченное объявление переменных, должна быть или ',' или ';'!", l);
                        }
                    } else if (lx == "begin") {
                        break;
                    } else
                        synterr("Неверный тип переменной '{0}'.", l, lx);
                }
                // BOD
                while (true) {
                    if (!Operator())
                        break;
                    else {
                        l = GetLex();
                        if (l.get() != ";")
                            synterr("Оператор должен заканчиваться символом ';'.", l);
                    }
                }
                l = SkipLex();
                if (l.code != 2 || l.get() != ".") synterr("Не найдена точка выхода из программы.", l);
                MessageBox.Show("Синтаксический анализ завершён успешно.");
            } catch (Exception ex) {
                string s = string.Empty;
                if (errcode == string.Empty)
                    s += ex.ToString();
                else
                    s += (errobj.Length > 0 ? String.Format(errcode, errobj) : errcode) +
                        "\nЛиния " + line + " в позиции " + linech + ".\n" + 
                        (_DEBUG ? ("Стек: \n" + ex.ToString()) : "");
                richTextBox1.Select(lid - wordsz, wordsz);
                MessageBox.Show(s, "Синтаксическая ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public enum VarType { none, real, integer, boolean }
        private lex? _SEMWR = null;
        private void FormIdent(string t, lex l) {
            if (l.type != VarType.none) {
                _SEMWR = l;
                return;
            }
            switch(t){
                case "integer": {
                        l.type = VarType.integer;
                        Types["integer"].Add(l);
                        dataGridView3.Rows[l.chr-1].Cells[1].Value = "I";
                        break;
                    }
                case "real": {
                        l.type = VarType.real;
                        Types["real"].Add(l);
                        dataGridView3.Rows[l.chr - 1].Cells[1].Value = "R";
                        break;
                    }
                case "bool": {
                        l.type = VarType.boolean;
                        Types["bool"].Add(l);
                        dataGridView3.Rows[l.chr - 1].Cells[1].Value = "B";
                        break;
                    }
                default:
                        break;
            }
        }

        private void dataGridView4_CellContentClick(object sender, DataGridViewCellEventArgs e) {

        }

        private void MorfVerify() {
            try {
                if (_SEMWR != null) {
                    synterr("Переопределение идентификатора.", _SEMWR.Value);
                    return;
                }
            } catch (Exception ex) {
                string s = string.Empty;
                if (errcode == string.Empty)
                    s += ex.ToString();
                else
                    s += errobj.Length > 0 ? String.Format(errcode, errobj) : errcode;
                MessageBox.Show(s, "Морфологическая ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public Dictionary<string, List<lex>> Types = new Dictionary<string, List<lex>>();
        private void синтаксическийАнализToolStripMenuItem_Click(object sender, EventArgs e) {
            SyntVerify();
        }
        private void лексическийАнализToolStripMenuItem_Click(object sender, System.EventArgs e) {
            LexScan(richTextBox1.Text);
            LexVerify();
            /*int ln = lexems.Count;
            for (int i = 0; i < ln; i++)
                write(lsts[lexems[i].code-1][lexems[i].chr-1]);*/
        }
        private void морфологическийАнализToolStripMenuItem_Click(object sender, EventArgs e) {
            MorfVerify();
        }
    }
}