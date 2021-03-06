using System;
using System.Collections.Generic;

using Utils;
using Bank;
using BankUserIntefrace;
using BankEvent;
using CustomException;


namespace Human
{
    // just a human and nothing more
    [Serializable]
    public abstract class Person
    {

        public abstract string FirstName { get; }
        public abstract string LastName { get; }

        public abstract void ShowInfo();
    }

    [Serializable]
    public class User : Person, IComparable
    {
        public readonly long id;


        public string login { get; }
        public string password { get; }

        protected string firstName;
        protected string lastName;

        protected static long nextId;

        protected static readonly List<string> DefaultQuestions = new List<string>(){
            "Enter first name",
            "Enter last name",
            "Enter login",
            "Enter password"
        };

        public override string FirstName
        {
            get
            {
                return String_IO.strFirstCharToUpper(firstName);
            }
        }

        public override string LastName
        {
            get
            {
                return String_IO.strFirstCharToUpper(lastName);
            }
        }

        public int CompareTo(object obj)
        {
            User user = obj as User;
            if (user != null) 
            {
                return this.id.CompareTo(user.id);
            }
            else
            {
                throw new InvalidCastException("Impossible to compare two users. Probably type error");
            }
        }


        // public static User CreateUser()
        // {
        //     string firstName = String_IO.GetInputOnText("Enter first name");
        //     string lastName = String_IO.GetInputOnText("Enter last name");
        //     string login = String_IO.GetInputOnText("Enter login");
        //     string password = "";
        //     do
        //     {
        //         password = String_IO.GetHiddenString_IO("Enter password");
        //         string passwordToCheck = String_IO.GetHiddenString_IO("Enter password again");
        //         if (passwordToCheck != password)
        //         {
        //             Console.WriteLine("Error: passwords differ");
        //             continue;
        //         }
        //         break;
        //     } while (true);
        //     User newUser = new User(firstName, lastName, login, password);
        //     return newUser;
        // }

        static User()
        {
            nextId = 0;
        }

        public User(string firstName, string lastName, string login, string password)
        {
            this.id = nextId++;
            this.firstName = firstName;
            this.lastName = lastName;
            this.login = login;
            this.password = password;
        }

        public string FullName
        {
            get { return $"{this.FirstName} {this.LastName}"; }
        }

        //method to override
        public override void ShowInfo()
        {
            Console.WriteLine($"User info {this.id} - '{this.FullName}'");
        }

        protected static List<string> AnswerQuestions(List<string> questions)
        {
            var answers = new List<string>();
            foreach (var q in questions)
            {
                string lowerCaseQuestion = q.ToLower();
                string ans;
                if (lowerCaseQuestion.Contains("password"))
                {
                    ans = String_IO.GetHiddenConsoleInput(q);
                }
                else
                {
                    ans = String_IO.GetInputOnText(q);
                }
                answers.Add(ans);
            }
            return answers;
        }
    }
    [Serializable]
    class BankClient : User, IMoney, ISystem
    {
        private List<long> accountsIds; //agregation
        private string secretQuestion;
        private string secretAnswer;

        private event HandleSystemUserInfoRequest InfoReqEvent;

        public override void ShowInfo()
        {
            Console.WriteLine("Client info-------");
            base.ShowInfo();
            // foreach (var accId in accountsIds)
            // {
            //     Bank.BankSystem.GetAccountById(accId).ShowAmount();
            // }
            Console.WriteLine("Client info-------");
        }

        public void ShowInfo(string beginStr, string finishStr)
        {
            Console.WriteLine(beginStr);
            base.ShowInfo();
            foreach (var accId in accountsIds)
            {
                Bank.BankSystem.GetAccountById(accId).ShowAmount();
            }
            Console.WriteLine(finishStr);
        }

        private BankClient(string firstName,
                           string lastName,
                           string login,
                           string password,
                           string secretQuestion,
                           string secretAnswer) : base(firstName, lastName, login, password)
        {
            this.secretAnswer = secretAnswer;
            this.secretQuestion = secretQuestion;
            this.accountsIds = new List<long>();
            this.InfoReqEvent = new HandleSystemUserInfoRequest(BankSystem.GetEmployeeRigths);
        }


        public static BankClient CreateBankClient()
        {
            var basicAnswers = User.AnswerQuestions(User.DefaultQuestions);
            string secretQuestion = String_IO.GetInputOnText("Enter secret question");
            string secretAnswer = String_IO.GetHiddenConsoleInput("Enter secret answer");
            return new BankClient(basicAnswers[0], basicAnswers[1], basicAnswers[2], basicAnswers[3], secretQuestion, secretAnswer);
        }

        public void AddAccountId(long accId)
        {
            this.accountsIds.Add(accId);
        }

        public int TakeCredit(int moneyValue)
        {
            if (moneyValue <= 0) return 0;

            int accountIdWithCredit = -1;

            foreach (var accId in this.accountsIds)
            {
                var acc = BankSystem.GetAccountById(accId);
                if (acc.MoneyAmount > moneyValue)
                {
                    acc.IncreaseAmount(moneyValue);
                    accountIdWithCredit = (int)acc.id;
                    break;
                }
            }

            return accountIdWithCredit;
        }

        public bool ExchangeMoney(int accountSrcId, int accountDstId, int MoneyAmount)
        {
            var accSrc = BankSystem.GetAccountById(accountSrcId);
            var accDst = BankSystem.GetAccountById(accountDstId);

            if (!this.accountsIds.Contains(accountSrcId))
                return false;

            if (accSrc == null || accDst == null)
                return false;

            if (accSrc.DecreaseAmount(MoneyAmount))
            {
                accDst.IncreaseAmount(MoneyAmount);
            }
            else
            {
                return false;
            }
            return true;
        }

        public bool LeaveSystem()
        {
            return BankSystem.RemoveUser((int)this.id);
        }

        public void ShowPossibleSystemActions()
        {
            if (InfoReqEvent != null)
            {
                string actions = InfoReqEvent();
                Console.WriteLine(actions);
            }
            else
            {
                Console.WriteLine("No answer from system");
            }
        }

        void IMoney.ShowIntefaceActions()
        {
            Console.WriteLine("Take credit\nExchange money\nBank employee is not allowed to do such actions");
        }

        void ISystem.ShowIntefaceActions()
        {
            Console.WriteLine("LeaveSystem\nShowPossibleSystemActions - get possible actions to interact with system");
        }
    }
    [Serializable]
    class BankEmployee : User, IMoney, ISystem
    {
        public string Position { get; }


        public int SystemUsersCount
        {
            get
            {
                if (this.HasRights)
                {
                    return BankSystem.UsersСount;
                }
                else
                {
                    throw new EmployeeAccessException(new EmployeeAccessExceptionArgs(this, "Current employee has no rights"));
                }
            }
        }

        private Boolean HasRights;

        private event HandleSystemUserInfoRequest InfoReqEvent;


        public override void ShowInfo()
        {
            Console.WriteLine("Employee info-----");
            base.ShowInfo();
            Console.WriteLine($"Position {this.Position}");
            Console.WriteLine("Employee info-----");
        }

        public void ShowInfo(string beginStr, string finishStr)
        {
            Console.WriteLine(beginStr);
            base.ShowInfo();
            Console.WriteLine($"Position {this.Position}");
            Console.WriteLine(finishStr);
        }

        private BankEmployee(string firstName,
                             string lastName,
                             string login,
                             string password,
                             string Position,
                             Boolean HasRights) : base(firstName, lastName, login, password)
        {
            this.Position = Position;
            this.HasRights = HasRights;
            this.InfoReqEvent = new HandleSystemUserInfoRequest(BankSystem.GetEmployeeRigths);
        }

        public static BankEmployee CreateBankEmployee()
        {
            var basicAnswers = User.AnswerQuestions(User.DefaultQuestions);
            string position = String_IO.GetInputOnText("Enter your Position in bank");
            string bankPassword = String_IO.GetHiddenConsoleInput("Enter SUPER SECRET BANK PASSWORD");

            Boolean HasRights = (bankPassword == BankSystem.SecretPassword);

            if (HasRights)
                Console.WriteLine("Corerct password, access given");
            else
                Console.WriteLine("Wrong password, access denied");

            return new BankEmployee(basicAnswers[0], basicAnswers[1], basicAnswers[2], basicAnswers[3], position, HasRights);
        }



        // interface inheritance
        // IMoney
        public int TakeCredit(int moneyValue)
        {
            Console.WriteLine("ERROR: bank employee is not alowed to use its bank services");
            throw new EmployeeAccessException(new EmployeeAccessExceptionArgs(this, "Current employee has no rights"));
        }

        public bool ExchangeMoney(int accountSrcId, int accountDstId, int MoneyAmount)
        {
            if (!this.HasRights)
                throw new EmployeeAccessException(new EmployeeAccessExceptionArgs(this, "Current employee has no rights"));

            var accSrc = BankSystem.GetAccountById(accountSrcId);
            var accDst = BankSystem.GetAccountById(accountDstId);

            if (accSrc == null || accDst == null)
            {
                return false;
            }
            if (accSrc.DecreaseAmount(MoneyAmount))
            {
                accDst.IncreaseAmount(MoneyAmount);
            }
            else
            {
                return false;
            }
            return true;
        }
        //IMoney

        // ISystem
        public bool LeaveSystem()
        {
            return BankSystem.RemoveUser((int)this.id);
        }

        public void ShowPossibleSystemActions()
        {
            if (InfoReqEvent != null)
            {
                string actions = InfoReqEvent();
                Console.WriteLine(actions);
            }
            else
            {
                Console.WriteLine("No answer from system");
            }
        }
        // ISystem

        void IMoney.ShowIntefaceActions()
        {
            Console.WriteLine("Take credit\nExchange money. User is allowed to do this");
        }

        void ISystem.ShowIntefaceActions()
        {
            Console.WriteLine("LeaveSystem\nShowPossibleSystemActions - get possible actions to interact with system");
        }

        // interfacew
    }
}