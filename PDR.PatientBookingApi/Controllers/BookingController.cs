using Microsoft.AspNetCore.Mvc;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices;
using PDR.PatientBooking.Service.BookingServices.Requests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PDR.PatientBookingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            // As BookingController differed a lot from other controllers and was not easy
            // to update the decision was made to refactor it and introduce BookingService.
            // Refactoring should always be taken with caution, be agreed within the project
            // and made sure everything still works as expected.
            _bookingService = bookingService;
        }

        [HttpGet("patient/{identificationNumber}/next")]
        public IActionResult GetPatientNextAppointnemtn(long identificationNumber)
        {
            try
            {
                var appointment = _bookingService.GetNextBooking(identificationNumber);

                if (appointment == null)
                    return StatusCode(502);
                else
                    return Ok(appointment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [HttpPost()]
        public IActionResult AddBooking(AddBookingRequest request)
        {
            try
            {
                _bookingService.AddBooking(request);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        private static MyOrderResult UpdateLatestBooking(List<Order> bookings2, int i)
        {
            MyOrderResult latestBooking;
            latestBooking = new MyOrderResult();
            latestBooking.Id = bookings2[i].Id;
            latestBooking.DoctorId = bookings2[i].DoctorId;
            latestBooking.StartTime = bookings2[i].StartTime;
            latestBooking.EndTime = bookings2[i].EndTime;
            latestBooking.PatientId = bookings2[i].PatientId;
            latestBooking.SurgeryType = (int)bookings2[i].GetSurgeryType();

            return latestBooking;
        }

        private class MyOrderResult
        {
            public Guid Id { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public long PatientId { get; set; }
            public long DoctorId { get; set; }
            public int SurgeryType { get; set; }
        }
    }
}