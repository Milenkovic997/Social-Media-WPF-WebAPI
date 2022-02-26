using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Social_Media_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        
        private readonly DataContext _dataContext;
        public UsersController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        // USER CONTROLLS
        [HttpGet]
        public async Task<ActionResult<List<Users>>> Get()
        {
            return Ok(await _dataContext.Users.ToListAsync());
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Users>> Get(int id)
        {
            var user = await _dataContext.Users.FindAsync(id);
            if(user == null)
            {
                return BadRequest("404");
            }
            return Ok(user);
        }
        [HttpPost]
        public async Task<ActionResult<List<Users>>> AddUser(Users u)
        {
            _dataContext.Users.Add(u);
            await _dataContext.SaveChangesAsync();
            return Ok(await _dataContext.Users.ToListAsync());
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<List<Users>>> Put(Users request, int id)
        {
            Users user = await _dataContext.Users.FindAsync(id);
            if (user == null) return BadRequest("404");

            user.id = id;
            user.name = request.name;
            user.imageURL = request.imageURL;
            user.imageURLBG = request.imageURLBG;
            user.bio = request.bio; 
            user.livesIn = request.livesIn;
            user.relationship = request.relationship;

            await _dataContext.SaveChangesAsync(); 
            return Ok(await _dataContext.Users.ToListAsync());
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult<List<Users>>> Delete(int id)
        {
            var user = await _dataContext.Users.FindAsync(id);
            if (user == null) { return BadRequest("404"); }

            _dataContext.Users.Remove(user);
            await _dataContext.SaveChangesAsync();
            return Ok(await _dataContext.Users.ToListAsync());
        }

        // POST CONTROLLS
        [HttpGet("/api/posts")]
        public async Task<ActionResult<List<Posts>>> getPosts()
        {
            return Ok(await _dataContext.Posts.ToListAsync());

        }
        [HttpGet("/api/{userID}/posts")]
        public async Task<ActionResult<List<Posts>>> getPosts(int userID)
        {
            var list = await _dataContext.Posts.Where(p => p.userID == userID).ToListAsync();
            return Ok(list);

        }
        [HttpPost("/api/{userID}/posts")]
        public async Task<ActionResult<List<Posts>>> AddPosts(Posts p, int userID)
        {
            var user = await _dataContext.Users.FindAsync(userID);
            if (user == null) return BadRequest("404");
            p.userID = userID;

            _dataContext.Posts.Add(p);
            await _dataContext.SaveChangesAsync();

            var list = await _dataContext.Posts.Where(p => p.userID == userID).ToListAsync();
            return Ok(list);
        }
        [HttpPut("/api/posts")]
        public async Task<ActionResult<List<Posts>>> PutPosts(Posts request)
        {
            Posts p = await _dataContext.Posts.FindAsync(request.id);
            if (p == null) return BadRequest("404");

            p.id = request.id;
            p.userImage = request.userImage;

            await _dataContext.SaveChangesAsync();
            return Ok(await _dataContext.Posts.ToListAsync());
        }
        [HttpDelete("/api/{postId}/posts")]
        public async Task<ActionResult<List<Users>>> DeletePost(int postId)
        {
            var post = await _dataContext.Posts.FindAsync(postId);
            if (post == null) { return BadRequest("404"); }

            _dataContext.Posts.Remove(post);
            await _dataContext.SaveChangesAsync();
            return Ok(await _dataContext.Posts.ToListAsync());
        }

        // FOLLOWING
        [HttpGet("/api/following")]
        public async Task<ActionResult<List<Following>>> getFollowing()
        {
            return Ok(await _dataContext.Following.ToListAsync());
        }
        [HttpGet("/api/{userID}/following")]
        public async Task<ActionResult<List<Following>>> getFollowing(int userID)
        {
            var list = await _dataContext.Following.Where(p => p.userID == userID).ToListAsync();
            return Ok(list);
        }
        [HttpPost("/api/{userID}/following")]
        public async Task<ActionResult<List<Following>>> AddFollowing(Following p, int userID)
        {
            var user = await _dataContext.Users.FindAsync(userID);
            if (user == null) return BadRequest("404");
            p.userID = userID;

            _dataContext.Following.Add(p);
            await _dataContext.SaveChangesAsync();

            var list = await _dataContext.Following.Where(p => p.userID == userID).ToListAsync();
            return Ok(list);
        }
        [HttpPut("/api/following")]
        public async Task<ActionResult<List<Following>>> PutFollowing(Following request)
        {
            Following following = await _dataContext.Following.FindAsync(request.id);
            if (following == null) return BadRequest("404");

            following.id = request.id;
            following.userID = request.userID;
            following.followingID = request.followingID;
            following.name = request.name;
            following.image = request.image;

            await _dataContext.SaveChangesAsync();
            return Ok(await _dataContext.Following.ToListAsync());
        }
        [HttpDelete("/api/{followingId}/following")]
        public async Task<ActionResult<List<Following>>> DeleteFollowing(int followingId)
        {
            var following = await _dataContext.Following.FindAsync(followingId);
            if (following == null) { return BadRequest("404"); }

            _dataContext.Following.Remove(following);
            await _dataContext.SaveChangesAsync();
            return Ok(await _dataContext.Following.ToListAsync());
        }

        // MESSAGES
        [HttpGet("/api/messages")]
        public async Task<ActionResult<List<Messages>>> getMessages()
        {
            return Ok(await _dataContext.Messages.ToListAsync());
        }
        [HttpPost("/api/{userID}/messages")]
        public async Task<ActionResult<List<Messages>>> AddMessages(Messages p, int userID)
        {
            var user = await _dataContext.Users.FindAsync(userID);
            if (user == null) return BadRequest("404");
            p.fromID = userID;

            _dataContext.Messages.Add(p);
            await _dataContext.SaveChangesAsync();

            var list = await _dataContext.Messages.Where(p => p.fromID == userID).ToListAsync();
            return Ok(list);
        }
        [HttpPut("/api/messages")]
        public async Task<ActionResult<List<Messages>>> PutMessages(Messages request)
        {
            Messages m = await _dataContext.Messages.FindAsync(request.id);
            if (m == null) return BadRequest("404");

            m.id = request.id;
            m.fromImage = request.fromImage;
            m.toImage = request.toImage;

            await _dataContext.SaveChangesAsync();
            return Ok(await _dataContext.Messages.ToListAsync());
        }
    }
}
