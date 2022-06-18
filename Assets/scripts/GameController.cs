using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] private CharacterComponent[] playerCharacters;
    [SerializeField] private CharacterComponent[] enemyCharacters;

    private Coroutine gameLoop;
    private bool waitingForInput;
    private CharacterComponent currentTarget;
    private bool paused = false;
    private GameObject gameMenu;
    public TextMeshProUGUI resultText;

    [SerializeField]
    private PlaySound playSound;

    [SerializeField]
    private List<string> playSoundNames;

    private void Start()
    {
        waitingForInput = true;
        gameLoop = StartCoroutine(GameLoop());
        gameMenu = GameObject.Find("GameMenu");
        gameMenu.SetActive(false);
        if (playSound)
        {
            playSound.PlaySoundEffect(playSoundNames[0]);
        }
    }

    private IEnumerator GameLoop()
    {
        Coroutine turn = StartCoroutine(Turn(playerCharacters, enemyCharacters));

        yield return new WaitUntil(() =>
        playerCharacters.FirstOrDefault(c => !c.HealthComponent.IsDead) == null ||
        enemyCharacters.FirstOrDefault(c => !c.HealthComponent.IsDead) == null);

        StopCoroutine(turn);
        GameOver();
    }

    private CharacterComponent GetTarget(CharacterComponent[] characterComponents)
    {
        var characterComponent = characterComponents.FirstOrDefault(c => !c.HealthComponent.IsDead);
        if (characterComponent != null)
        {
            characterComponent.IndicatorComponent.EnableTargetIndicator();
        }

        return characterComponent;
    }

    private void GameOver()
    {
        bool isPlayerCharacherAlive = false;
        bool isEnemyCharacherAlive = false;

        bool isVictory;

        for (int i = 0; i < playerCharacters.Length; i++)
        {
            if (!playerCharacters[i].HealthComponent.IsDead)
            {
                isPlayerCharacherAlive = true;
            }
        }

        for (int i = 0; i < enemyCharacters.Length; i++)
        {
            if (!enemyCharacters[i].HealthComponent.IsDead)
            {
                isEnemyCharacherAlive = true;
            }
        }

        isVictory = isPlayerCharacherAlive && !isEnemyCharacherAlive;

        Debug.Log(isVictory ? "Victory" : "Defeat");
        if (isVictory == true)
        {
            resultText.color = Color.blue;
            resultText.text = "WIN!";
            playSound.StopSoundEffect(playSoundNames[0]);
            StartCoroutine(WinOrLoseSoundCoroutine(isVictory));
        }

        if (isVictory == false)
        {
            resultText.color = Color.red;
            resultText.text = "DEFEAT!";
            playSound.StopSoundEffect(playSoundNames[0]);
            StartCoroutine(WinOrLoseSoundCoroutine(isVictory));
        }
    }

    private IEnumerator Turn(CharacterComponent[] playerCharacters, CharacterComponent[]
        enemyCharacters)
    {
        int turnCounter = 0;
        while (true)
        {
            foreach (var player in playerCharacters)
            {

                if (currentTarget == null)
                {
                    currentTarget = GetTarget(enemyCharacters);
                }

                while (waitingForInput)
                {
                    yield return null;
                }

                if (player.HealthComponent.IsDead)
                {
                    Debug.Log("Character: " + player.name + " is dead");
                    continue;
                }

                player.SetTarget(currentTarget.HealthComponent);

                //TODO: hotfix
                yield return null; // ugly fix need to investigate
                player.StartTurn();
                yield return new WaitUntilCharacterAttack(player);

                Debug.Log("Character: " + player.name + " finished turn");
                waitingForInput = true;
                currentTarget.IndicatorComponent.DisableTargetIndicator();
                currentTarget = null;
            }


            yield return new WaitForSeconds(.5f);
            foreach (var enemy in enemyCharacters)
            {
                if (enemy.HealthComponent.IsDead)
                {
                    Debug.Log("Enemy character: " + enemy.name + " is dead");
                    continue;
                }

                var characterComponent = GetTarget(playerCharacters);
                enemy.SetTarget(characterComponent.HealthComponent);
                enemy.StartTurn();

                yield return new WaitUntilCharacterAttack(enemy);
                if (enemy._weapon.Weapon == Weapon.Bat)
                {
                    yield return new WaitForSeconds(2f);
                }

                if (enemy._weapon.Weapon == Weapon.Kulak)
                {
                    yield return new WaitForSeconds(3.2f);
                }
                Debug.Log("Enemy character: " + enemy.name + " finished turn");
                characterComponent.IndicatorComponent.DisableTargetIndicator();
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            waitingForInput = false;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            NextTarget();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (!paused)
            {
                Time.timeScale = 0;
                paused = true;
            }
            else
            {
                Time.timeScale = 1;
                paused = false;
            }
        }
    }

    public void PlayerAttack()
    {
        waitingForInput = false;
    }

    public void SwitchEnemy()
    {
        NextTarget();
    }

    public void OpenGameMenu()
    {
        if (!paused)
        {
            Time.timeScale = 0;
            paused = true;
            gameMenu.SetActive(true);
        }
    }

    public void ContinueGame()
    {
        if (paused)
        {
            Time.timeScale = 1;
            paused = false;
            gameMenu.SetActive(false);
        }
    }

    public void OpenMainMenu()
    {
        if (paused)
        {
            Time.timeScale = 1;
            paused = false;
            gameMenu.SetActive(false);
            SceneManager.LoadScene("Scenes/Menu");
        }
        else
        {
            SceneManager.LoadScene("Scenes/Menu");
        }
    }

    public void StartGame()
    {
        if (paused)
        {
            Time.timeScale = 1;
            paused = false;
            gameMenu.SetActive(false);
            SceneManager.LoadScene("Scenes/Game");
        }
        else
        {
            SceneManager.LoadScene("Scenes/Game");
        }
    }

    private void NextTarget()
    {
        int index = Array.IndexOf(enemyCharacters, currentTarget);
        for (int i = 1; i < enemyCharacters.Length; i++)
        {
            int next = (index + i) % enemyCharacters.Length;
            if (!enemyCharacters[next].HealthComponent.IsDead)
            {
                currentTarget.IndicatorComponent.DisableTargetIndicator();
                currentTarget = enemyCharacters[next];
                currentTarget.IndicatorComponent.EnableTargetIndicator();
                return;
            }
        }
    }

    private IEnumerator WinOrLoseSoundCoroutine(bool isVictory)
    {
        yield return new WaitForSeconds(0.3f);
        if (isVictory == true)
        {
            playSound.PlaySoundEffect(playSoundNames[1]);
        }

        if (isVictory == false)
        {
            playSound.PlaySoundEffect(playSoundNames[2]);
        }
    }
}
