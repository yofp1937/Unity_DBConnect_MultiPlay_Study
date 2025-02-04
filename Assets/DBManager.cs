using System.Collections;
using System.Collections.Generic;
using System.IO;
using MongoDB.Bson;
using MongoDB.Driver;
using TMPro;
using UnityEngine;

public class DBManager : MonoBehaviour
{
    // MongoDB 연결 변수
    private IMongoClient client;
    private IMongoDatabase database;
    private IMongoCollection<BsonDocument> collection; // Bson은 MongoDB의 데이터를 다루는 형식인 Binary JSON의 약자임, BsonDocument는 C#에서 Bson을 표현하기위한 클래스

    public TMP_InputField Input_Id;
    private string id;
    public TMP_InputField Input_Pwd;
    private string pwd;

    #region "Singleton"
    public static DBManager instance;
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    void Init() // Awake 동작할때 실행됨
    {
        // MongoDB 연결 주소 불러옴(깃허브에서 보이지 않기위해 외부 파일에서 주소를 불러옴)
        string dbaddress = ReadDBConnectionAddress();
        ConnectDataBase(dbaddress);
    }

    // MongoDB 연결 주소 불러오는 함수
    private string ReadDBConnectionAddress()
    {
        // Assets/Security/AppSettings.txt를 path에 저장
        string path = Path.Combine(Application.dataPath, "Security", "AppSettings.txt");
        string dbAddress = "";

        if(File.Exists(path))
        {
            // dbAddress에 AppSettings에 써있는 mongodb_address 주소를 저장
            dbAddress = File.ReadAllText(path).Replace("mongodb_address=", "").Trim();
        }
        else
        {
            Debug.LogError("AppSettings.txt not found!");
        }

        return dbAddress;
    }

    private void ConnectDataBase(string DBAddress)
    {
        // MongoDB 클라이언트(접속자) 생성
        client = new MongoClient(DBAddress);
        // 데이터베이스 선택
        database = client.GetDatabase("UserData");
        // 컬렉션 선택(테이블)
        collection = database.GetCollection<BsonDocument>("UserInfo");
    }

    void ReadData()
    {
        var filter = new BsonDocument();  // 데이터를 가져오기 위한 변수
        var documents = collection.Find(filter).ToList(); // collection에 지정된 컬렉션의 모든 BSON 데이터를 List형태로 가져옴

        // 가져온 모든 데이터를 출력
        if (documents.Count > 0)
        {
            foreach (var doc in documents)
            {
                Debug.Log("Document: " + doc);
            }
        }
        else
        {
            Debug.Log("No documents found.");
        }
    }

    // SignUp Btn과 연결
    public void RequestSignUP()
    {
        if(LoadInputField())
        {
            SignUp();
        }
    }
    // 회원가입 함수
    private void SignUp()
    {
        Debug.Log($"회원가입 요청 - Id:{id}, Pwd:{pwd}");
        // 유저 Id 중복인지 확인
        var filter = Builders<BsonDocument>.Filter.Eq("Id", id);
        var checkuser = collection.Find(filter).FirstOrDefault();

        if(checkuser != null)
        {
            Debug.LogError("회원가입 실패: 이미 존재하는 Id입니다");
            return;
        }

        // 새로운 유저 등록
        var newuser = new BsonDocument
        {
            { "Id", id },
            { "Password", pwd }  // 실제 프로젝트에서는 암호화 진행 필수
        };
        collection.InsertOne(newuser);
        ResetInputField(true, true);
        Debug.Log($"{id}님 회원가입 완료");
    }

    // Login Btn과 연결
    public void RequestLogin()
    {
        if(LoadInputField())
        {
            Login();
        }
    }
    // 로그인 함수
    private void Login()
    {
        Debug.Log($"로그인 요청 - Id:{id}, Pwd:{pwd}");
        // Id, Pwd 일치 확인
        var filter = Builders<BsonDocument>.Filter.And(
            Builders<BsonDocument>.Filter.Eq("Id", id),
            Builders<BsonDocument>.Filter.Eq("Password", pwd) // 암호화된건 복호화 필수
        );

        var checkuser = collection.Find(filter).FirstOrDefault();

        if(checkuser != null)
        {
            Debug.Log($"{id}님 로그인 성공"); // 일치하면 로그인
            ResetInputField(true, true);
        }
        else
        {
            Debug.LogError("로그인 실패: 잘못된 Id 혹은 Password"); // 불일치시 에러
            ResetInputField(false, true);
        }
    }

    // 사용자가 입력한 아이디, 비밀번호 읽어오는 함수
    bool LoadInputField()
    {
        id = Input_Id.text;
        pwd = Input_Pwd.text;

        if(string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(pwd))
        {
            Debug.LogError("Id 혹은 Password를 제대로 입력하세요");
            return false;
        }

        return true;
    }

    // 회원가입, 로그인 성공시 InputField 지우는 용도
    void ResetInputField(bool boolid, bool boolpwd) // true 들어온쪽만 InputField 비움
    {
        if(boolid)
        {
            Input_Id.text = "";
        }

        if(boolpwd)
        {
            Input_Pwd.text = "";
        }
    }
}
