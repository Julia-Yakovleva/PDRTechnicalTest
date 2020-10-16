using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PDR.PatientBooking.Service.Tests.BookingServices.Validation
{
    [TestFixture]
    public class AddBookingRequestValidatorTests
    {
        private Fixture _fixture;
        private PatientBookingContext _context;
        private AddBookingRequestValidator _addBookingRequestValidator;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _fixture = new Fixture();

            //Prevent fixture from generating from entity circular references 
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            // Mock setup
            _context = new PatientBookingContext(new DbContextOptionsBuilder<PatientBookingContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            // Sut instantiation
            _addBookingRequestValidator = new AddBookingRequestValidator(_context);
        }

        [Test]
        public void ValidateRequest_StartTimeInThePast_ReturnsFailedValidation()
        {
            //arrange
            var request = GetValidRequest();
            request.StartTime = DateTime.UtcNow.AddDays(-1);

            //act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain("Appointment should start in the future");
        }

        [Test]
        public void ValidateRequest_StartTimeGreaterEndTime_ReturnsFailedValidation()
        {
            //arrange
            var request = GetValidRequest();
            request.StartTime = DateTime.UtcNow.AddDays(1);

            //act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain("Start time should be prior to end time");
        }

        [Test]
        public void ValidateRequest_DoctorDoesNotExist_ReturnsFailedValidation()
        {
            //arrange
            var request = GetValidRequest();
            var notExistingDoctorId = 8888;
            request.DoctorId = notExistingDoctorId;

            //act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain("The doctor not found");
        }

        [Test]
        public void ValidateRequest_PatientDoesNotExist_ReturnsFailedValidation()
        {
            //arrange
            var request = GetValidRequest();
            var notExistingPatientId = 9999;
            request.PatientId = notExistingPatientId;

            //act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain("The patient not found");
        }

        [TestCase(0, 0)]
        [TestCase(-1, -1)]
        [TestCase(-1, 1)]
        [TestCase(1, -1)]
        [TestCase(1, 1)]
        public void ValidateRequest_TimeIntervalIntersects_ReturnsFailedValidation(int startTimeDelta, int endTimeDelta)
        {
            //arrange
            var request = GetExistingBookingCloneRequest();
            request.StartTime.AddHours(startTimeDelta);
            request.EndTime.AddHours(endTimeDelta);

            //act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain("The doctor is busy");
        }

        [Test]
        public void ValidateRequest_ValidRequest_ReturnsPassedValidation()
        {
            //arrange
            var request = GetValidRequest();

            //act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeTrue();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ValidateRequest_NeighboringTimeInterval_ReturnsPassedValidation(bool isPreceding)
        {
            //arrange
            var request = GetExistingBookingCloneRequest();

            var existingStartTime = request.StartTime;
            var existingEndTime = request.EndTime;

            request.StartTime = isPreceding? existingStartTime.AddHours(-1) : existingEndTime;
            request.EndTime = isPreceding ? existingStartTime : existingEndTime.AddHours(1);

            //act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeTrue();
        }

        private AddBookingRequest GetValidRequest()
        {
            var doctor = _fixture.Build<Doctor>()
                .With(x => x.Id, 1)
                .Without(x => x.Orders)
                .Create();
            var patient = _fixture.Build<Patient>()
                .With(x => x.Id, 2)
                .Without(x => x.Orders)
                .Create();
            
            _context.Add(doctor);
            _context.Add(patient);
            _context.SaveChanges();

            var request = _fixture.Build<AddBookingRequest>()
                .With(x => x.DoctorId, doctor.Id)
                .With(x => x.PatientId, patient.Id)
                .With(x => x.StartTime, DateTime.UtcNow.AddHours(10))
                .With(x => x.EndTime, DateTime.UtcNow.AddHours(15))
                .Create();

            return request;
        }

        private AddBookingRequest GetExistingBookingCloneRequest()
        {
            var booking = _fixture.Build<Order>()
                .With(x => x.StartTime, DateTime.UtcNow.AddHours(10))
                .With(x => x.EndTime, DateTime.UtcNow.AddHours(15))
                .With(x => x.IsCancelled, false)
                .Create();

            _context.Add(booking);
            _context.SaveChanges();

            return _fixture.Build<AddBookingRequest>()
                .With(x => x.DoctorId, booking.DoctorId)
                .With(x => x.PatientId, _context.Patient.First().Id)
                .With(x => x.StartTime, booking.StartTime)
                .With(x => x.EndTime, booking.EndTime)
                .Create();
        }
    }
}
