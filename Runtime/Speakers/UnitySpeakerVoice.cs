using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using jmayberry.CustomAttributes;

namespace jmayberry.TypewriterHelper {
	/**
	 * Uses the built-in Unity audio to play a dialog sound.
	 * Use: [How to Create Dialogue Audio like in Celeste and Animal Crossing using Unity](https://www.youtube.com/watch?v=P3FcXHEai_E&list=PLlWoaOaxVGvz1ogeTzol1rHBlRKxmVIS_&index=55&t=5s)
	 */
	[CreateAssetMenu(fileName = "NewSpeakerVoice", menuName = "Typewriter/UnitySpeakerVoice", order = 1)]
	public class UnitySpeakerVoice : BaseSpeakerVoice {
		[Header("Setup")]
		public AudioClip[] dialogueTypingSoundClips;
		[Range(0f, 1.5f)] public float speed = 1f;
		[Range(-3f, 3f)] public float minPitch = 0.5f;
		[Range(-3f, 3f)] public float maxPitch = 3f;
		public bool makePredictable = true;
		public bool stopAudioSource = true;

		[Header("Debug")]
		[Readonly] public float lastSoundTime = 0f;
		[Readonly] public float secondsPerSound;
		private AudioSource audioSource;

		public override void NewConversation() {
			lastSoundTime = 0f;
			this.secondsPerSound = 1 / (this.speed * 5);
		}

		public void SetAudioSource(AudioSource audioSource) {
			this.audioSource = audioSource;
		}

		public virtual void PlaySound(AudioSource audioSource, int currentDisplayedCharacterCount, char currentCharacter) {
			this.SetAudioSource(audioSource);
			this.PlaySound(currentDisplayedCharacterCount, currentCharacter);
		}

		public override void PlaySound(int currentDisplayedCharacterCount, char currentCharacter) {
			if ((Time.time - lastSoundTime) < secondsPerSound) {
				return;
			}
			lastSoundTime = Time.time;

			if (stopAudioSource) {
				audioSource.Stop();
			}

			if (!makePredictable) {
				audioSource.pitch = Random.Range(minPitch, maxPitch);
				audioSource.PlayOneShot(dialogueTypingSoundClips[Random.Range(0, dialogueTypingSoundClips.Length)]);
				return;
			}

			int hashCode = currentCharacter.GetHashCode();
			int minPitchInt = (int)(minPitch * 100);
			int maxPitchInt = (int)(maxPitch * 100);
			int pitchRange = maxPitchInt - minPitchInt;

			if (pitchRange != 0) {
				int predictablePitchInt = (hashCode % pitchRange) + minPitchInt;
				float predictablePitch = predictablePitchInt / 100f;
				audioSource.pitch = predictablePitch;
			}
			else {
				audioSource.pitch = minPitch;
			}

			audioSource.PlayOneShot(dialogueTypingSoundClips[hashCode % dialogueTypingSoundClips.Length]);
		}

		public override void StopSound() {
			audioSource.Stop();
		}
	}
}