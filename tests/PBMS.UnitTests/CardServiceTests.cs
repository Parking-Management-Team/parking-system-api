using NSubstitute;
using PBMS.Application.Card.DTOs;
using PBMS.Application.Card.Services;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
using PBMS.Domain.Exceptions;

namespace PBMS.UnitTests
{
    public class CardServiceTests
    {
        private readonly ICardRepository _cardRepositoryMock;
        private readonly CardService _cardService;

        public CardServiceTests()
        {
            // 1. Arrange: Khởi tạo Mock Object cho ICardRepository
            _cardRepositoryMock = Substitute.For<ICardRepository>();
            
            // 2. Arrange: Inject Mock vào CardService
            _cardService = new CardService(_cardRepositoryMock);
        }

        // =========================================================================
        // UNIT TESTS CHO TÍNH NĂNG TẠO THẺ (Scenario 1)
        // =========================================================================

        [Fact]
        public async Task CreateCardAsync_ShouldCreateCard_WhenCardCodeIsUnique()
        {
            // Arrange
            var request = new CreateCardRequest 
            { 
                CardCode = "card-123", 
                CardType = "PARKING_CARD",
                RfidCode = "rfid-123"
            };

            // Thiết lập Mock: Trả về false khi kiểm tra xem CARD-123 đã tồn tại chưa
            _cardRepositoryMock.IsCardCodeExistsAsync("CARD-123").Returns(false);

            // Act
            var result = await _cardService.CreateCardAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CARD-123", result.CardCode); // Code phải được Trim và UpperCase
            Assert.Equal("rfid-123", result.RfidCode);
            Assert.Equal("PARKING_CARD", result.CardType);
            Assert.Equal("Available", result.CardStatus); // Trạng thái mặc định

            // Kiểm tra xem repository có được gọi đúng các hàm lưu trữ hay không
            await _cardRepositoryMock.Received(1).AddAsync(Arg.Is<Card>(c => 
                c.CardCode == "CARD-123" && 
                c.CardStatus == "Available"
            ));
            await _cardRepositoryMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task CreateCardAsync_ShouldThrowDomainException_WhenCardCodeAlreadyExists()
        {
            // Arrange
            var request = new CreateCardRequest { CardCode = "card-existing" };
            
            // Thiết lập Mock: Báo là thẻ này đã tồn tại trong DB rồi
            _cardRepositoryMock.IsCardCodeExistsAsync("CARD-EXISTING").Returns(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(() => 
                _cardService.CreateCardAsync(request)
            );

            Assert.Equal("CARD_CODE_EXISTS", exception.ErrorCode);
            
            // Đảm bảo không gọi hàm thêm mới và hàm lưu vào DB
            await _cardRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Card>());
            await _cardRepositoryMock.DidNotReceive().SaveChangesAsync();
        }

        // =========================================================================
        // UNIT TESTS CHO TÍNH NĂNG XÓA THẺ (Scenario 2)
        // =========================================================================

        [Fact]
        public async Task DeleteCardAsync_ShouldDeleteCard_WhenCardIsFree()
        {
            // Arrange
            int cardId = 99;
            var card = new Card { Id = cardId, CardCode = "CARD-FREE" };

            // Thiết lập Mock: Tìm thấy thẻ và thẻ này KHÔNG bận đỗ xe
            _cardRepositoryMock.GetByIdAsync(cardId).Returns(card);
            _cardRepositoryMock.IsCardInActiveSessionAsync(cardId).Returns(false);

            // Act
            await _cardService.DeleteCardAsync(cardId);

            // Assert
            await _cardRepositoryMock.Received(1).RemoveAsync(card);
            await _cardRepositoryMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task DeleteCardAsync_ShouldThrowDomainException_WhenCardIsUsedInActiveSession()
        {
            // Arrange
            int cardId = 100;
            var card = new Card { Id = cardId, CardCode = "CARD-BUSY" };

            // Thiết lập Mock: Tìm thấy thẻ và thẻ này ĐANG bận đỗ xe
            _cardRepositoryMock.GetByIdAsync(cardId).Returns(card);
            _cardRepositoryMock.IsCardInActiveSessionAsync(cardId).Returns(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(() => 
                _cardService.DeleteCardAsync(cardId)
            );

            Assert.Equal("CARD_IN_ACTIVE_SESSION", exception.ErrorCode);

            // Đảm bảo hàm Remove không được gọi
            await _cardRepositoryMock.DidNotReceive().RemoveAsync(Arg.Any<Card>());
            await _cardRepositoryMock.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task UpdateCardStatusAsync_ShouldUpdateStatusAndSetLostAt_WhenStatusIsLost()
        {
            // Arrange
            int cardId = 5;
            var card = new Card { Id = cardId, CardCode = "CARD-5", CardStatus = CardStatus.Available.ToString() };
            _cardRepositoryMock.GetByIdAsync(cardId).Returns(card);

            // Act
            var result = await _cardService.UpdateCardStatusAsync(cardId, "Lost");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Lost", result.CardStatus);
            Assert.NotNull(card.LostAt);
            _cardRepositoryMock.Received(1).Update(card);
            await _cardRepositoryMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task UpdateCardStatusAsync_ShouldClearLostAt_WhenStatusIsNoLongerLost()
        {
            // Arrange
            int cardId = 5;
            var card = new Card { Id = cardId, CardCode = "CARD-5", CardStatus = CardStatus.Lost.ToString(), LostAt = DateTime.UtcNow };
            _cardRepositoryMock.GetByIdAsync(cardId).Returns(card);

            // Act
            var result = await _cardService.UpdateCardStatusAsync(cardId, "Available");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Available", result.CardStatus);
            Assert.Null(card.LostAt);
        }
    }
}

