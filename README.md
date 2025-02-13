[![Watch the video on Youtube ](https://img.youtube.com/vi/<video_id>/0.jpg)](https://www.youtube.com/watch?v=CNsRncRb2Iw)

<img width="667" alt="image" src="https://github.com/user-attachments/assets/7c55564f-609c-4a1c-b3b1-b22de3b2081a" />
<img width="566" alt="image" src="https://github.com/user-attachments/assets/0e056a2a-8a21-4854-8e98-08743593fc1a" />

<iframe width="560" height="315" src="https://www.youtube.com/embed/CNsRncRb2Iw?si=4RNWis1wKff2TQey" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" referrerpolicy="strict-origin-when-cross-origin" allowfullscreen></iframe>


<h3>For Developer Account App Registration follow below steps,</h3>
<ul>
  <li>on https://developer.linkedin.com/ create an App </li>
  <img width="886" alt="image" src="https://github.com/user-attachments/assets/f5a696d0-3fa3-4fa2-8515-e550158b1220" />
<img width="523" alt="image" src="https://github.com/user-attachments/assets/3040ab6d-3447-4114-9cf7-7fc0e2f8d542" />

Once After Creation can check all apps on https://www.linkedin.com/developers/apps/
Then on clicking the app will appear like below,
<img width="557" alt="image" src="https://github.com/user-attachments/assets/49ef64db-f080-4f88-932a-328c2a0b6ccf" />
Since its not verified ,so Shows Verify. Once verified will get more features,scopes through that we can do more scopes.
Once verified will become like below,
<img width="542" alt="image" src="https://github.com/user-attachments/assets/72d16ae7-55d4-4079-abf9-cbc1e3cbfba5" />

For Code clientid,secret required. Can be extracted from below,

<img width="450" alt="image" src="https://github.com/user-attachments/assets/4b171478-e67b-465d-8d00-7c28f5de48b8" />

Here had to set authorized url,where application will be using with redirect url, for after login where it should be back. It should be matched otherwise will not work.
By default no scopes are enabled, THen press Request Access
<img width="440" alt="image" src="https://github.com/user-attachments/assets/dc3ad6d2-7603-4aca-9a5d-40a4720889d1" />

Below 3 are necessary to post,

<img width="426" alt="image" src="https://github.com/user-attachments/assets/6e35997d-f58c-4beb-ab83-27590490de88" />


Once scopes all are ready, then on code
<img width="827" alt="image" src="https://github.com/user-attachments/assets/0fdcea93-9f82-4088-80b7-829936cc29bc" />

Please make sure of config clientid and secret with exact redirect url
<img width="917" alt="image" src="https://github.com/user-attachments/assets/f46fe580-cbe7-4a93-bbdb-1ab0faa0f358" />


It goes in 3 steps,
<ul>
  <li>1.on share page on click it passes text,imageurl of remote,url to add on text and on share <li>
  <li>2.call goes to https://www.linkedin.com/oauth/v2/authorization and gets code in back to callback url if all matches</li>
<li>3.on linedkin/callback endpoint, it extracts auth token, then user self id before posting then uploads image if passed then share posts with user confirmation.</li>
</ul>
<img width="625" alt="image" src="https://github.com/user-attachments/assets/55583bf5-69ed-417f-b4c2-9268562c1408" />










