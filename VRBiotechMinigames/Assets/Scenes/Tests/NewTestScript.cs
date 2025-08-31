using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Events;
using Object = UnityEngine.Object;
using System;
[TestFixture]
public class NewTestScript
{
    private GameObject testerObject;
    [SetUp]
    public void SetUp ()
    {
        testerObject = new GameObject("Tester Object");
    }
    
    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(testerObject);
    }

    [Test]
    public void DoSomething_WhenCalled_InvokesEvent()
    {
        EventPublisher eventPublisher = testerObject.AddComponent<EventPublisher>();
        eventPublisher.SomethingHappened = new UnityEvent<object, EventArgs>();

        bool isEventInvoked =false;

        eventPublisher.SomethingHappened.AddListener((arg0, args) =>
		{
			isEventInvoked = true;
		});

        eventPublisher.DoSomething();

        Assert.That(isEventInvoked, Is.True);

    }
}

internal class EventPublisher: MonoBehaviour
{
    public UnityEvent<object, EventArgs> SomethingHappened;
    public void DoSomething()
    {
        OnSomethingHappened();
    }

    protected virtual void OnSomethingHappened()
    {
        SomethingHappened?.Invoke(this, EventArgs.Empty);
    }
}