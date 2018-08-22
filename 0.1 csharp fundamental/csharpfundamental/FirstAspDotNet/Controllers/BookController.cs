using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FirstAspDotNet.Controllers
{
    /// <summary>
    /// nghiep quan ly sach
    /// </summary>
    public class BookController : ApiController
    {
        /// <summary>
        /// Chi tiet cuon sach
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public Response<BookDetail> Get(Request<BookRequest> request)
        {
            if (request.Access_Token == "")
            {
                return new Response<BookDetail>()
                {
                    ErrorCode= 403,
                    Message="no authenticate",
                    Data=null
                };
            }
            //get from mssql map entity to bookdetail
            //return new BookDetail()
            //{
            //    Id = request.Id,
            //    Name = "C# in action"
            //};

            return new Response<BookDetail>()
            {
                ErrorCode = 0,
                Message = "ok",
                Data = new BookDetail()
                {
                    Id = request.Data.Id,
                    Name = "C# in action"
                }
            };
        }
    }



    /// <summary>
    /// response for detail
    /// </summary>
    public class BookDetail
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// request for detail
    /// </summary>
    public class BookRequest
    {
        public Guid Id { get; set; }
    }


    public class Request<T>
    {
        public T Data { get; set; }

        public string Access_Token { get; set; }
    }

    public class Response<T>
    {
        public int ErrorCode { get; set; } = 0;
        public string Message { get; set; }
        public T Data { get; set; }
    }

    public class NoAuthenticate{
        }
}
