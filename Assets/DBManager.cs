using System.Collections;
using System.Collections.Generic;
using System.IO;
using MongoDB.Bson;
using MongoDB.Driver;
using UnityEngine;

public class DBManager : MonoBehaviour
{
    // MongoDB 연결 변수
    private IMongoClient client;
    private IMongoDatabase database;
    private IMongoCollection<BsonDocument> collection; // Bson은 MongoDB의 데이터를 다루는 형식인 Binary JSON의 약자임, BsonDocument는 C#에서 Bson을 표현하기위한 클래스

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
        database = client.GetDatabase("sample_mflix");
        // 컬렉션 선택(테이블)
        collection = database.GetCollection<BsonDocument>("comments");

        // TestCode
        Debug.Log("Client: " + client);
        Debug.Log("DataBase: " + database);
        Debug.Log("Collection: " + collection);

        // 데이터베이스에서 데이터 읽기 테스트
        ReadData();
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
}
