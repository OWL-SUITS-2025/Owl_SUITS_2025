import requests

# 1. Upload audio file
with open(r"C:\Users\camer\Downloads\Recording.wav", "rb") as f:
    files = {'audio': ("Recording.wav", f, "audio/wav")}
    response = requests.post("http://3.144.242.35:7070/audio", files=files)
    print("Upload response:", response.text)
    audio_id = response.json()['id']

# 2. Add a POI referencing the audio_id
poi_payload = {
    "name": "Test POI",
    "x": 1.23,
    "y": 4.56,
    "tags": [],
    "description": "POI with audio",
    "type": "test-type",
    "audioId": audio_id
}
poi_response = requests.post("http://3.144.242.35:7070/poi", json=poi_payload)
print("POI add response:", poi_response.text)

# 3. Retrieve POIs and extract the audioId from the first POI
get_pois = requests.get("http://3.144.242.35:7070/poi")
print("POIs:", get_pois.text)
poi_list = get_pois.json()
if not poi_list:
    raise Exception("No POIs found!")
first_poi = poi_list[0]
retrieved_audio_id = first_poi.get("audioId")
print("Retrieved POI audioId:", retrieved_audio_id)

# 4. Retrieve the audio file by the audioId from the POI
audio_response = requests.get(f"http://3.144.242.35:7070/audio/{retrieved_audio_id}")
print("Get audio by POI audioId status:", audio_response.status_code)
with open("retrieved_test_from_poi.wav", "wb") as out:
    out.write(audio_response.content)

print("Retrieved audio from POI saved as retrieved_test_from_poi.wav")