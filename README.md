
# CsCensorAPI


## API Reference

### send a Image and recive JSON output

```http
  POST /Censorship
```

| form-data key | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `"file"` | `Image` | **Required**.  |

```json
{
  "Hentai": 0.0012763405,
  "Neutral": 0.0022586877,
  "Pornography": 0.90473783,
  "Sexy": 0.09172714,
  "PredictedLabel": "Pornography",
  "IsNsfw": true
}
```

### send a Video and recive JSON output

```http
  POST /Censorship
```

| form-data key | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `"file"` | `Video` | **Required**.  |

```json
[
  {
    "FilePath": "/home/ubuntu/CsCensorApi/outDir/frame_0003.png",
    "Result": {
      "Hentai": 0.053215913,
      "Neutral": 0.002230461,
      "Pornography": 0.49808654,
      "Sexy": 0.4464671,
      "PredictedLabel": "Pornography",
      "IsNsfw": true
    }
  },
  {
    "FilePath": "/home/ubuntu/CsCensorApi/outDir/frame_0001.png",
    "Result": {
      "Hentai": 0.03023044,
      "Neutral": 0.017910523,
      "Pornography": 0.5663283,
      "Sexy": 0.38553077,
      "PredictedLabel": "Pornography",
      "IsNsfw": true
    }
  },
  {
    "FilePath": "/home/ubuntu/CsCensorApi/outDir/frame_0002.png",
    "Result": {
      "Hentai": 0.020706583,
      "Neutral": 0.7398937,
      "Pornography": 0.120232575,
      "Sexy": 0.119167216,
      "PredictedLabel": "Neutral",
      "IsNsfw": false
    }
  }
]
```

## Run Locally

Clone the project

```bash
  git clone https://github.com/TomerTeichGit/CsCensorApi.git
```

Go to the project directory

```bash
  cd CsCensorApi
```
Install dependencies

```bash
  dotnet add package NsfwSpy 
  sudo apt install ffmpeg
```

Start the server

```bash
  dotnet run --launch-profile http
```
## Notes
* The controller uses the NSFW Spy library for image and video classification.
* Images are classified individually, while videos are first split into frames before classification.

## Authors

- [@TomerTeich](https://www.github.com/TomerTeichGit)
