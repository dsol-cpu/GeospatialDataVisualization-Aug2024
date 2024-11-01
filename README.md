This was a code challenge project done for AES in order to:
* Parse dirty JSON containing location names and corresponding longitude/latitude values
* Map them to a globe
* Represent them in an interesting way

Solution:
* I decided to parse with a regex to catch all cases in the dirty JSON
* used Unity Jobs and the Burst compiler to render and dynamically rotate all pins on the globe

After the fact:
* I added an automatic CI/CD pipeline to build the game in WebGL and deploy the game on github pages (Website linked!)
* https://dsol-cpu.github.io/GeospatialDataVisualization-Aug2024/

Tools used: Unity, C#, Blender

Video:
[![](https://i.ytimg.com/vi_webp/coCoAvOaSBM/maxresdefault.webp)](http://www.youtube.com/watch?v=coCoAvOaSBM&feature=emb_title)
