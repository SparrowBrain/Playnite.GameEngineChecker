using AutoFixture.Xunit2;
using FakeItEasy;
using GameEngineChecker.Interfaces;
using GameEngineChecker.Services;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestTools.Shared;
using Xunit;

namespace GameEngineChecker.UnitTests.Services
{
	public class PersistentProcessingQueueTests
	{
		[Theory]
		[AutoFakeItEasyData]
		public async Task Enqueue_WritesToFile_WhenItemsAdded(
			[Frozen] IQueuePersistence queuePersistenceMock,
			List<Guid> gameIds,
			PersistentProcessingQueue sut)
		{
			// Act
			await sut.Enqueue(gameIds);

			// Assert
			A.CallTo(() => queuePersistenceMock.Save(gameIds)).MustHaveHappened();
		}

		[Theory]
		[AutoFakeItEasyData]
		public async Task Enqueue_WritesToFileAllGames_WhenItemsAddedAndGamesWereAlreadyInTheQueue(
			[Frozen] IQueuePersistence queuePersistenceMock,
			List<Guid> oldGameIds,
			List<Guid> newGameIds,
			PersistentProcessingQueue sut)
		{
			// Arrange
			await sut.Enqueue(oldGameIds);

			// Act
			await sut.Enqueue(newGameIds);

			// Assert
			A.CallTo(() =>
					queuePersistenceMock.Save(
						A<IReadOnlyCollection<Guid>>.That.Matches(g => oldGameIds.Concat(newGameIds).All(g.Contains))))
				.MustHaveHappened();
		}

		[Theory]
		[AutoFakeItEasyData]
		public async Task Constructor_LoadsFromFile_WhenIsCreated(
			[Frozen] IPlayniteAPI playniteApiMock,
			[Frozen] IQueuePersistence queuePersistenceMock,
			List<Guid> oldGameIds,
			List<Guid> newGameIds)
		{
			// Arrange
			A.CallTo(() => queuePersistenceMock.Load()).Returns(oldGameIds);

			// Act
			var sut = new PersistentProcessingQueue(queuePersistenceMock, x => Task.CompletedTask);
			await sut.Enqueue(newGameIds);

			// Assert
			A.CallTo(() => queuePersistenceMock.Save(
				A<IReadOnlyCollection<Guid>>.That.Matches(g => oldGameIds.Concat(newGameIds).All(g.Contains))))
				.MustHaveHappened();
		}

		[Theory]
		[AutoFakeItEasyData]
		public async Task Process_ExecutesTheAction(
			[Frozen] IPlayniteAPI playniteApiMock,
			[Frozen] IQueuePersistence queuePersistenceMock)
		{
			// Arrange
			var actionCalled = false;
			var semaphore = new SemaphoreSlim(0, 1);
			var sut = new PersistentProcessingQueue(queuePersistenceMock, x =>
			{
				actionCalled = true;
				semaphore.Release();
				return Task.CompletedTask;
			});

			// Act
			sut.ProcessInBackground();

			// Assert
			await semaphore.WaitAsync(TimeSpan.FromSeconds(5));
			Assert.True(actionCalled);
		}

		[Theory]
		[AutoFakeItEasyData]
		public async Task Process_SavesFile_WhenActionExecutes(
			[Frozen] IPlayniteAPI playniteApiMock,
			[Frozen] IQueuePersistence queuePersistenceMock,
			List<Guid> gameIds)
		{
			// Arrange
			var semaphore = new SemaphoreSlim(0, 1);
			var sut = new PersistentProcessingQueue(queuePersistenceMock, x =>
			{
				semaphore.Release();
				return Task.CompletedTask;
			});
			await sut.Enqueue(gameIds);

			// Act
			sut.ProcessInBackground();

			// Assert
			await semaphore.WaitAsync(TimeSpan.FromSeconds(5));
			A.CallTo(() => queuePersistenceMock.Save(A<IReadOnlyCollection<Guid>>.That.Matches(c => c.Count == 0)))
				.MustHaveHappened();
		}

		[Theory]
		[AutoFakeItEasyData]
		public async Task Process_DoesNotSave_WhenActionFails(
			[Frozen] IPlayniteAPI playniteApiMock,
			[Frozen] IQueuePersistence queuePersistenceMock,
			List<Guid> gameIds)
		{
			// Arrange
			var semaphore = new SemaphoreSlim(0, 1);
			var sut = new PersistentProcessingQueue(queuePersistenceMock, x => throw new Exception());
			await sut.Enqueue(gameIds);

			// Act
			sut.ProcessInBackground();

			// Assert
			await semaphore.WaitAsync(TimeSpan.FromSeconds(1));
			A.CallTo(() => queuePersistenceMock.Save(A<IReadOnlyCollection<Guid>>.That.Matches(c => c.Count == 0))).MustNotHaveHappened();
		}

		[Theory]
		[AutoFakeItEasyData]
		public async Task Process_ItemsStayInQueue_WhenActionFails(
			[Frozen] IPlayniteAPI playniteApiMock,
			[Frozen] IQueuePersistence queuePersistenceMock,
			List<Guid> gameIds)
		{
			// Arrange
			var semaphore = new SemaphoreSlim(0, 1);
			var sut = new PersistentProcessingQueue(queuePersistenceMock, x => throw new Exception());
			await sut.Enqueue(gameIds);
			Fake.Reset(queuePersistenceMock);

			// Act
			sut.ProcessInBackground();

			// Assert
			await semaphore.WaitAsync(TimeSpan.FromSeconds(1));
			A.CallTo(() =>
			queuePersistenceMock.Save(A<IReadOnlyCollection<Guid>>.That.Matches(g => gameIds.All(g.Contains))))
				.MustHaveHappened();
		}
	}
}