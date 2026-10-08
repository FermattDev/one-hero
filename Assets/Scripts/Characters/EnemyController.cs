using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private ProjectileController projectilePrefab;
    [SerializeField] private float detectionRange = 3;
    [SerializeField] protected float fireDelay = 1f;

    private PlayerController playerController;
    private List<KeyValuePair<float, KeyValuePair<string, object>>> actionRecorder = new List<KeyValuePair<float, KeyValuePair<string, object>>>();

    private List<KeyValuePair<float, KeyValuePair<string, object>>> actionPlayer = new List<KeyValuePair<float, KeyValuePair<string, object>>>();
    private bool actionStart = false;
    private float timeStart;
    private Task fireTask;
    private int index = 0;

    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float hitRadius = 0.5f;
    [SerializeField] private Vector3 lifebarOffset = new Vector3(0, 0.8f, 0);
    [SerializeField] private Vector2 lifebarSize = new Vector2(1f, 0.12f);

    public static readonly List<EnemyController> Active = new List<EnemyController>();

    private int health;
    private Transform lifebarRoot;
    private Transform lifebarFill;
    private SpriteRenderer lifebarFillRenderer;
    private static Sprite whiteSprite;

    private void Awake()
    {
        health = maxHealth;
        CreateLifebar();
    }

    private void OnEnable()
    {
        Active.Add(this);
    }

    private void OnDisable()
    {
        Active.Remove(this);
    }

    private void OnDestroy()
    {
        if (lifebarRoot != null)
        {
            Destroy(lifebarRoot.gameObject);
        }
    }

    public bool IsHitBy(Vector3 point)
    {
        point.z = transform.position.z;
        return (point - transform.position).sqrMagnitude <= hitRadius * hitRadius;
    }

    public void TakeDamage(int amount)
    {
        health = Mathf.Max(0, health - amount);
        UpdateLifebar();

        if (health == 0)
        {
            AvatarDead();
        }
    }

    private static Sprite GetWhiteSprite()
    {
        if (whiteSprite == null)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            whiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0, 0.5f), 1f);
        }
        return whiteSprite;
    }

    private void CreateLifebar()
    {
        // Not parented to the enemy so it does not rotate with it.
        lifebarRoot = new GameObject("Lifebar").transform;

        var background = new GameObject("Background").AddComponent<SpriteRenderer>();
        background.transform.SetParent(lifebarRoot, false);
        background.sprite = GetWhiteSprite();
        background.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        background.sortingOrder = 100;
        background.transform.localPosition = new Vector3(-lifebarSize.x / 2f, 0, 0);
        background.transform.localScale = new Vector3(lifebarSize.x, lifebarSize.y, 1);

        lifebarFillRenderer = new GameObject("Fill").AddComponent<SpriteRenderer>();
        lifebarFill = lifebarFillRenderer.transform;
        lifebarFill.SetParent(lifebarRoot, false);
        lifebarFillRenderer.sprite = GetWhiteSprite();
        lifebarFillRenderer.sortingOrder = 101;
        lifebarFill.localPosition = new Vector3(-lifebarSize.x / 2f, 0, 0);

        UpdateLifebar();
        LateUpdate();
    }

    private void UpdateLifebar()
    {
        float ratio = maxHealth > 0 ? (float)health / maxHealth : 0f;
        lifebarFill.localScale = new Vector3(lifebarSize.x * ratio, lifebarSize.y, 1);

        // Green (100%) -> yellow (50%) -> red (0%)
        lifebarFillRenderer.color = ratio >= 0.5f
            ? Color.Lerp(Color.yellow, Color.green, (ratio - 0.5f) * 2f)
            : Color.Lerp(Color.red, Color.yellow, ratio * 2f);
    }

    private void LateUpdate()
    {
        if (lifebarRoot != null)
        {
            lifebarRoot.position = transform.position + lifebarOffset;
            lifebarRoot.rotation = Quaternion.identity;
        }
    }

    private void Start()
    {
        var levelManager = ManagerLocator.Get<LevelManager>();
        levelManager.OnLevelReset += ResetEnemy;
        playerController = levelManager.GetPlayerController();
    }

    public void ResetEnemy(PlayerController player)
    {
        playerController = player;
        actionStart = true;
        timeStart = Time.time;
        index = 0;
    }

    protected virtual async Task AvatarFire(object value)
    {
        Vector3 direction = (Vector3)value;

        var projectile = Instantiate(projectilePrefab);
        projectile.transform.position = transform.position;
        projectile.SetProjectileDirection(direction.normalized);
        projectile.SetProjectileCreator(transform);

        if(!actionStart)
        {
            AddActionToRecorder("Fire", true);
        }

        await Task.Delay((int)(fireDelay * 1000));
    }

    private void AddActionToRecorder(string key, object value)
    {
        var actionValue = new KeyValuePair<string, object>(key, value);
        var action = new KeyValuePair<float, KeyValuePair<string, object>>(Time.time - timeStart, actionValue);
        actionRecorder.Add(action);
    }

    protected virtual void AvatarDead()
    {
        Destroy(gameObject);

        if (!actionStart)
        {
            AddActionToRecorder("Dead", true);
        }
    }

    private void Update()
    {
        if (!actionStart)
        {
            if(playerController == null || (fireTask != null && !fireTask.IsCompleted))
            {
                return;
            }
            if((playerController.transform.position - transform.position).magnitude < detectionRange)
            {
                Vector3 direction = playerController.transform.position - transform.position;
                Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);
                transform.rotation = rotation;

                fireTask = AvatarFire(direction);
            }
            return;
        }
        if (actionPlayer.Count > index)
        {
            if ((actionPlayer[index].Key) < Time.time - timeStart)
            {
                switch (actionPlayer[index].Value.Key)
                {
                    case "Fire":
                        fireTask = AvatarFire(actionPlayer[index].Value.Value);
                        break;
                    case "Dead":
                        AvatarDead();
                        break;
                }

                index++;
            }
        }
        else
        {
            actionStart = false;
        }
    }
}
