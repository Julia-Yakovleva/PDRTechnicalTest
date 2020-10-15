using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Validation;
using PDR.PatientBooking.Service.Validation;
using System;

namespace PDR.PatientBooking.Service.Tests.BookingServices
{
    [TestFixture]
    public class BookingServiceTests
    {
        private MockRepository _mockRepository;
        private Fixture _fixture;
        private PatientBookingContext _context;
        private Mock<IAddBookingRequestValidator> _validator;
        private BookingService _bookingService;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _fixture = new Fixture();

            //Prevent fixture from generating circular references
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            // Mock setup
            _context = new PatientBookingContext(new DbContextOptionsBuilder<PatientBookingContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            _validator = _mockRepository.Create<IAddBookingRequestValidator>();

            // Sut instantiation
            _bookingService = new BookingService(
                _context,
                _validator.Object
            );
        }

        [Test]
        public void CancelBooking_BookingIsNotCancelled_SetsIsCancelled()
        {
            //arrange
            var booking = _fixture.Build<Order>().With(o => o.IsCancelled, false).Create();
            _context.Order.Add(booking);
            _context.SaveChanges();

            //act
            _bookingService.CancelBooking(booking.Id);

            //assert
            booking.IsCancelled.Should().BeTrue();
        }

        [Test]
        public void CancelBooking_BookingAlreadyCancelled_ThrowsArgumentException()
        {
            //arrange
            var booking = _fixture.Build<Order>().With(o => o.IsCancelled, true).Create();
            _context.Order.Add(booking);
            _context.SaveChanges();

            //act
            var exception = Assert.Throws<ArgumentException>(() => _bookingService.CancelBooking(booking.Id));

            //assert
            exception.Message.Should().Be("Booking does not exist");
        }

        [Test]
        public void CancelBooking_BookingDoesNotExist_ThrowsArgumentException()
        {
            //arrange
            var notExistingBookingId = Guid.NewGuid();

            //act
            var exception = Assert.Throws<ArgumentException>(() => _bookingService.CancelBooking(notExistingBookingId));

            //assert
            exception.Message.Should().Be("Booking does not exist");
        }
    }
}
